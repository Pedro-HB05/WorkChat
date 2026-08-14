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
[Route("api/usuarios")]
public sealed class UsuariosController(
    WorkChatDbContext db,
    PasswordHashService passwordHash,
    DistribuicaoService distribuicao,
    ILogger<UsuariosController> logger) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.Administrador)]
    [HttpGet]
    public async Task<ActionResult<PaginaResponse<UsuarioResponse>>> Listar([FromQuery] PaginacaoQuery paginacao, CancellationToken ct)
    {
        var query = db.Usuarios.AsNoTracking().Where(x => x.EmpresaId == User.EmpresaId()).OrderBy(x => x.Nome);
        var total = await query.CountAsync(ct);
        var itens = await query.Skip((paginacao.Pagina - 1) * paginacao.Tamanho).Take(paginacao.Tamanho).Select(ResponseExpression).ToListAsync(ct);
        return Ok(new PaginaResponse<UsuarioResponse>(itens, paginacao.Pagina, paginacao.Tamanho, total));
    }

    [Authorize(Policy = AuthorizationPolicies.Equipe)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        if (!User.IsInRole(ChatConstants.PerfilAdmin) && User.UsuarioId() != id) return Forbid();
        var x = await db.Usuarios.AsNoTracking().Where(x => x.Id == id && x.EmpresaId == User.EmpresaId()).Select(ResponseExpression).FirstOrDefaultAsync(ct);
        return x is null ? NotFound() : Ok(x);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<UsuarioResponse>> Criar(CreateUsuarioRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var perfil = NormalizarPerfil(request.Perfil);
        if (perfil is null) return BadRequest("Perfil deve ser Admin ou Agent.");
        if (!await db.Empresas.AnyAsync(x => x.Id == request.EmpresaId && x.Ativa, ct)) return NotFound("Empresa ativa nao encontrada.");

        var empresaPossuiUsuarios = await db.Usuarios.AnyAsync(x => x.EmpresaId == request.EmpresaId, ct);
        if (empresaPossuiUsuarios)
        {
            if (User.Identity?.IsAuthenticated != true || !User.IsInRole(ChatConstants.PerfilAdmin) || User.EmpresaId() != request.EmpresaId) return Forbid();
        }
        else if (perfil != ChatConstants.PerfilAdmin)
        {
            return BadRequest("O primeiro usuario da empresa deve ser ADMIN.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(x => x.EmpresaId == request.EmpresaId && x.Email == email, ct)) return Conflict("Ja existe um usuario com este e-mail.");
        var x = new Usuario { EmpresaId = request.EmpresaId, Nome = request.Nome.Trim(), Email = email, SenhaHash = passwordHash.Hash(request.Senha), Perfil = perfil, LimiteChats = request.LimiteChats, DataCriacao = DateTime.UtcNow };
        db.Add(x); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        logger.LogInformation("Usuario {UsuarioId} criado na empresa {EmpresaId}", x.Id, x.EmpresaId);
        return CreatedAtAction(nameof(ObterPorId), new { id = x.Id }, CriarResponse(x));
    }

    [Authorize(Policy = AuthorizationPolicies.Administrador)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, UpdateUsuarioRequest request, CancellationToken ct)
    {
        var perfil = NormalizarPerfil(request.Perfil);
        if (perfil is null) return BadRequest("Perfil deve ser Admin ou Agent.");
        var x = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == User.EmpresaId(), ct); if (x is null) return NotFound();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.EmpresaId == x.EmpresaId && u.Email == email && u.Id != id, ct)) return Conflict("E-mail ja esta em uso.");
        x.Nome = request.Nome.Trim(); x.Email = email; x.Perfil = perfil; x.Ativo = request.Ativo; x.LimiteChats = request.LimiteChats;
        await db.SaveChangesAsync(ct); return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Administrador)]
    [HttpPost("{id:guid}/setores")]
    public async Task<IActionResult> VincularSetor(Guid id, VincularUsuarioSetorRequest request, CancellationToken ct)
    {
        var empresaId = User.EmpresaId();
        if (!await db.Usuarios.AnyAsync(x => x.Id == id && x.EmpresaId == empresaId, ct) || !await db.Setores.AnyAsync(x => x.Id == request.SetorId && x.EmpresaId == empresaId, ct)) return NotFound("Usuario ou setor nao encontrado.");
        if (!await db.UsuarioSetores.AnyAsync(x => x.UsuarioId == id && x.SetorId == request.SetorId, ct)) { db.Add(new UsuarioSetor { UsuarioId = id, SetorId = request.SetorId }); await db.SaveChangesAsync(ct); }
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Administrador)]
    [HttpDelete("{id:guid}/setores/{setorId:guid}")]
    public async Task<IActionResult> RemoverSetor(Guid id, Guid setorId, CancellationToken ct)
    {
        var empresaId = User.EmpresaId();
        var x = await db.UsuarioSetores.FirstOrDefaultAsync(x => x.UsuarioId == id && x.SetorId == setorId && x.Usuario!.EmpresaId == empresaId, ct);
        if (x is null) return NotFound(); db.Remove(x); await db.SaveChangesAsync(ct); return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Equipe)]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> AtualizarStatus(Guid id, AtualizarStatusUsuarioRequest request, CancellationToken ct)
    {
        if (!User.IsInRole(ChatConstants.PerfilAdmin) && User.UsuarioId() != id) return Forbid();
        var status = NormalizarStatus(request.Status);
        if (status is null) return BadRequest("Status invalido.");
        var x = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == User.EmpresaId(), ct); if (x is null) return NotFound();
        x.StatusAtendimento = status; await db.SaveChangesAsync(ct);
        if (status is ChatConstants.PresencaOnline or ChatConstants.PresencaOcupado)
        {
            var setores = await db.UsuarioSetores.Where(x => x.UsuarioId == id).Select(x => x.SetorId).ToListAsync(ct);
            foreach (var setorId in setores) await distribuicao.DistribuirProximoAsync(x.EmpresaId, setorId, ct);
            await db.SaveChangesAsync(ct);
        }
        return NoContent();
    }

    private static readonly System.Linq.Expressions.Expression<Func<Usuario, UsuarioResponse>> ResponseExpression = x =>
        new(x.Id, x.EmpresaId, x.Nome, x.Email, x.Perfil, x.Ativo, x.StatusAtendimento, x.LimiteChats, x.DataCriacao);
    private static UsuarioResponse CriarResponse(Usuario x) => new(x.Id, x.EmpresaId, x.Nome, x.Email, x.Perfil, x.Ativo, x.StatusAtendimento, x.LimiteChats, x.DataCriacao);

    private static string? NormalizarPerfil(string valor) => valor.Trim().ToUpperInvariant() switch
    {
        "ADMIN" => ChatConstants.PerfilAdmin,
        "AGENT" or "ATENDENTE" => ChatConstants.PerfilAtendente,
        _ => null
    };

    private static string? NormalizarStatus(string valor) => valor.Trim().ToUpperInvariant() switch
    {
        "ONLINE" => ChatConstants.PresencaOnline,
        "BUSY" or "OCUPADO" => ChatConstants.PresencaOcupado,
        "PAUSE" or "PAUSA" => ChatConstants.PresencaPausa,
        "AWAY" or "AUSENTE" => ChatConstants.PresencaAusente,
        "OFFLINE" => ChatConstants.PresencaOffline,
        _ => null
    };
}
