using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkChat.Authentication;
using WorkChat.Data;
using WorkChat.DTOs;
using WorkChat.Models;
using WorkChat.Services;

namespace WorkChat.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.Chat)]
[Route("api/conversas")]
public sealed class ConversasController(WorkChatDbContext db, DistribuicaoService distribuicao, ILogger<ConversasController> logger) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.Equipe)]
    [HttpGet]
    public async Task<ActionResult<PaginaResponse<ConversaResponse>>> Listar([FromQuery] string? status, [FromQuery] Guid? setorId, [FromQuery] PaginacaoQuery paginacao, CancellationToken ct)
    {
        var empresaId = User.EmpresaId();
        var query = db.Conversas.AsNoTracking().Where(x => x.EmpresaId == empresaId);
        if (User.IsInRole(ChatConstants.PerfilAtendente))
        {
            var usuarioId = User.UsuarioId()!.Value;
            query = query.Where(x => x.AtendenteId == usuarioId || x.AtendenteId == null && x.Setor!.UsuarioSetores.Any(us => us.UsuarioId == usuarioId));
        }
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == NormalizarStatus(status));
        if (setorId.HasValue) query = query.Where(x => x.SetorId == setorId.Value);

        var total = await query.CountAsync(ct);
        var conversas = await query.OrderByDescending(x => x.Prioridade).ThenByDescending(x => x.DataAbertura)
            .Skip((paginacao.Pagina - 1) * paginacao.Tamanho).Take(paginacao.Tamanho)
            .Include(x => x.Cliente).Include(x => x.Setor).Include(x => x.Atendente).ToListAsync(ct);
        var itens = new List<ConversaResponse>(conversas.Count);
        foreach (var x in conversas) itens.Add(CriarResponse(x, await PosicaoFila(x, ct)));
        return Ok(new PaginaResponse<ConversaResponse>(itens, paginacao.Pagina, paginacao.Tamanho, total));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversaResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var x = await QueryAcessivel().AsNoTracking().Include(x => x.Cliente).Include(x => x.Setor).Include(x => x.Atendente).FirstOrDefaultAsync(x => x.Id == id, ct);
        return x is null ? NotFound() : Ok(CriarResponse(x, await PosicaoFila(x, ct)));
    }

    [Authorize(Roles = ChatConstants.PerfilCliente)]
    [HttpPost]
    public async Task<ActionResult<ConversaResponse>> Criar(CreateConversaRequest request, CancellationToken ct)
    {
        var empresaId = User.EmpresaId(); var clienteId = User.ClienteId()!.Value;
        if (!await db.Clientes.AnyAsync(x => x.Id == clienteId && x.EmpresaId == empresaId, ct)
            || !await db.Setores.AnyAsync(x => x.Id == request.SetorId && x.EmpresaId == empresaId && x.Ativo, ct)) return NotFound("Cliente ou setor ativo nao encontrado.");

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var x = new Conversa { EmpresaId = empresaId, ClienteId = clienteId, SetorId = request.SetorId, Status = ChatConstants.StatusAguardando, Prioridade = request.Prioridade, DataAbertura = DateTime.UtcNow };
        db.Add(x); await distribuicao.TentarDistribuirAsync(x, ct); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        logger.LogInformation("Conversa {ConversaId} criada na empresa {EmpresaId}", x.Id, empresaId);
        return CreatedAtAction(nameof(ObterPorId), new { id = x.Id }, CriarResponse(x));
    }

    [Authorize(Policy = AuthorizationPolicies.Equipe)]
    [HttpPost("{id:guid}/assumir")]
    public async Task<IActionResult> Assumir(Guid id, AssumirConversaRequest request, CancellationToken ct)
    {
        if (User.IsInRole(ChatConstants.PerfilAtendente) && User.UsuarioId() != request.AtendenteId) return Forbid();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var conversa = await db.Conversas.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == User.EmpresaId(), ct); if (conversa is null) return NotFound();
        if (conversa.Status == ChatConstants.StatusEncerrada) return BadRequest("Conversa encerrada nao pode ser assumida.");
        var atendente = await db.Usuarios.Include(x => x.UsuarioSetores).FirstOrDefaultAsync(x => x.Id == request.AtendenteId && x.EmpresaId == conversa.EmpresaId && x.Ativo && x.Perfil == ChatConstants.PerfilAtendente, ct);
        if (atendente is null || !atendente.UsuarioSetores.Any(x => x.SetorId == conversa.SetorId)) return BadRequest("Atendente nao pertence ao setor da conversa.");
        if (atendente.StatusAtendimento is not (ChatConstants.PresencaOnline or ChatConstants.PresencaOcupado)) return Conflict("Atendente nao esta disponivel.");
        if (await db.Conversas.CountAsync(x => x.AtendenteId == atendente.Id && x.Status == ChatConstants.StatusEmAtendimento, ct) >= atendente.LimiteChats) return Conflict("Atendente atingiu o limite de chats.");
        conversa.AtendenteId = atendente.Id; conversa.Status = ChatConstants.StatusEmAtendimento; conversa.DataInicioAtendimento ??= DateTime.UtcNow; atendente.UltimaDistribuicao = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Equipe)]
    [HttpPost("{id:guid}/transferir")]
    public async Task<ActionResult<TransferenciaConversaResponse>> Transferir(Guid id, TransferirConversaRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var conversa = await QueryAcessivel().FirstOrDefaultAsync(x => x.Id == id, ct); if (conversa is null) return NotFound();
        if (conversa.Status == ChatConstants.StatusEncerrada) return BadRequest("Conversa encerrada nao pode ser transferida.");
        if (!await db.Setores.AnyAsync(x => x.Id == request.SetorDestinoId && x.EmpresaId == conversa.EmpresaId && x.Ativo, ct)) return BadRequest("Setor de destino invalido.");
        if (request.AtendenteDestinoId.HasValue && !await db.UsuarioSetores.AnyAsync(x => x.UsuarioId == request.AtendenteDestinoId && x.SetorId == request.SetorDestinoId && x.Usuario!.EmpresaId == conversa.EmpresaId && x.Usuario.Ativo, ct)) return BadRequest("Atendente de destino nao pertence ao setor.");
        var transferencia = new TransferenciaConversa { ConversaId = id, SetorOrigemId = conversa.SetorId, SetorDestinoId = request.SetorDestinoId, AtendenteOrigemId = conversa.AtendenteId, AtendenteDestinoId = request.AtendenteDestinoId, Motivo = request.Motivo?.Trim(), DataTransferencia = DateTime.UtcNow };
        conversa.SetorId = request.SetorDestinoId; conversa.AtendenteId = request.AtendenteDestinoId; conversa.Status = request.AtendenteDestinoId.HasValue ? ChatConstants.StatusEmAtendimento : ChatConstants.StatusAguardando;
        if (!request.AtendenteDestinoId.HasValue) await distribuicao.TentarDistribuirAsync(conversa, ct);
        db.Add(transferencia); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        logger.LogInformation("Conversa {ConversaId} transferida para o setor {SetorId}", id, request.SetorDestinoId);
        return Ok(new TransferenciaConversaResponse(transferencia.Id, id, transferencia.SetorOrigemId, transferencia.SetorDestinoId, transferencia.AtendenteOrigemId, transferencia.AtendenteDestinoId, transferencia.Motivo, transferencia.DataTransferencia));
    }

    [HttpGet("{id:guid}/transferencias")]
    public async Task<ActionResult<IEnumerable<TransferenciaConversaResponse>>> Transferencias(Guid id, CancellationToken ct)
    {
        if (!await QueryAcessivel().AnyAsync(x => x.Id == id, ct)) return NotFound();
        return Ok(await db.TransferenciasConversa.AsNoTracking().Where(x => x.ConversaId == id).OrderBy(x => x.DataTransferencia)
            .Select(x => new TransferenciaConversaResponse(x.Id, x.ConversaId, x.SetorOrigemId, x.SetorDestinoId, x.AtendenteOrigemId, x.AtendenteDestinoId, x.Motivo, x.DataTransferencia)).ToListAsync(ct));
    }

    [Authorize(Policy = AuthorizationPolicies.Equipe)]
    [HttpPost("{id:guid}/encerrar")]
    public async Task<IActionResult> Encerrar(Guid id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var x = await QueryAcessivel().FirstOrDefaultAsync(x => x.Id == id, ct); if (x is null) return NotFound();
        if (x.Status == ChatConstants.StatusEncerrada) return NoContent();
        x.Status = ChatConstants.StatusEncerrada; x.DataEncerramento = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        if (x.SetorId.HasValue) await distribuicao.DistribuirProximoAsync(x.EmpresaId, x.SetorId.Value, ct);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        logger.LogInformation("Conversa {ConversaId} encerrada", id); return NoContent();
    }

    private IQueryable<Conversa> QueryAcessivel()
    {
        var query = db.Conversas.Where(x => x.EmpresaId == User.EmpresaId());
        if (User.IsInRole(ChatConstants.PerfilCliente)) { var clienteId = User.ClienteId()!.Value; return query.Where(x => x.ClienteId == clienteId); }
        if (User.IsInRole(ChatConstants.PerfilAtendente)) { var usuarioId = User.UsuarioId()!.Value; return query.Where(x => x.AtendenteId == usuarioId || x.AtendenteId == null && x.Setor!.UsuarioSetores.Any(us => us.UsuarioId == usuarioId)); }
        return query;
    }

    private async Task<int?> PosicaoFila(Conversa x, CancellationToken ct) => x.Status != ChatConstants.StatusAguardando ? null :
        await db.Conversas.CountAsync(c => c.EmpresaId == x.EmpresaId && c.SetorId == x.SetorId && c.Status == ChatConstants.StatusAguardando && (c.Prioridade > x.Prioridade || c.Prioridade == x.Prioridade && c.DataAbertura < x.DataAbertura), ct) + 1;

    private static ConversaResponse CriarResponse(Conversa x, int? posicao = null) => new(x.Id, x.EmpresaId, x.ClienteId, x.Cliente?.Nome, x.Cliente?.Email, x.SetorId, x.Setor?.Nome, x.AtendenteId, x.Atendente?.Nome, x.Status, x.Prioridade, posicao, x.DataAbertura, x.DataInicioAtendimento, x.DataEncerramento);

    private static string NormalizarStatus(string status) => status.Trim().ToUpperInvariant() switch
    {
        "AGUARDANDO" or "WAITING" => ChatConstants.StatusAguardando,
        "EM_ATENDIMENTO" or "ACTIVE" => ChatConstants.StatusEmAtendimento,
        "ENCERRADA" or "CLOSED" => ChatConstants.StatusEncerrada,
        _ => status.Trim()
    };
}
