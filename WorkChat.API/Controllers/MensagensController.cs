using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WorkChat.Authentication;
using WorkChat.Data;
using WorkChat.DTOs;
using WorkChat.Hubs;
using WorkChat.Models;

namespace WorkChat.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.Chat)]
[Route("api/conversas/{conversaId:guid}/mensagens")]
public sealed class MensagensController(WorkChatDbContext db, IHubContext<ChatHub> hub, ILogger<MensagensController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginaResponse<MensagemResponse>>> Listar(Guid conversaId, [FromQuery] PaginacaoQuery paginacao, CancellationToken ct)
    {
        if (!await ConversaAcessivel(conversaId).AnyAsync(ct)) return NotFound("Conversa nao encontrada.");
        var query = db.Mensagens.AsNoTracking().Where(x => x.ConversaId == conversaId).OrderBy(x => x.DataEnvio);
        var total = await query.CountAsync(ct);
        var itens = await query.Skip((paginacao.Pagina - 1) * paginacao.Tamanho).Take(paginacao.Tamanho)
            .Select(x => new MensagemResponse(x.Id, x.ConversaId, x.RemetenteTipo, x.UsuarioId, x.ClienteId, x.Conteudo, x.DataEnvio)).ToListAsync(ct);
        return Ok(new PaginaResponse<MensagemResponse>(itens, paginacao.Pagina, paginacao.Tamanho, total));
    }

    [HttpPost]
    public async Task<ActionResult<MensagemResponse>> Enviar(Guid conversaId, EnviarMensagemRequest request, CancellationToken ct)
    {
        var conversa = await ConversaAcessivel(conversaId).FirstOrDefaultAsync(ct);
        if (conversa is null) return NotFound("Conversa nao encontrada.");
        if (conversa.Status == ChatConstants.StatusEncerrada) return BadRequest("Nao e possivel enviar mensagens em conversa encerrada.");

        var cliente = User.IsInRole(ChatConstants.PerfilCliente);
        var mensagem = new Mensagem
        {
            ConversaId = conversaId,
            RemetenteTipo = cliente ? ChatConstants.RemetenteCliente : ChatConstants.RemetenteUsuario,
            UsuarioId = cliente ? null : User.UsuarioId(),
            ClienteId = cliente ? User.ClienteId() : null,
            Conteudo = request.Conteudo.Trim(),
            DataEnvio = DateTime.UtcNow
        };
        db.Add(mensagem); await db.SaveChangesAsync(ct);
        var response = new MensagemResponse(mensagem.Id, mensagem.ConversaId, mensagem.RemetenteTipo, mensagem.UsuarioId, mensagem.ClienteId, mensagem.Conteudo, mensagem.DataEnvio);
        await hub.Clients.Group(ChatHub.GrupoConversa(conversaId)).SendAsync("MensagemRecebida", response, ct);
        logger.LogDebug("Mensagem {MensagemId} enviada na conversa {ConversaId}", mensagem.Id, conversaId);
        return CreatedAtAction(nameof(Listar), new { conversaId }, response);
    }

    private IQueryable<Conversa> ConversaAcessivel(Guid conversaId)
    {
        var query = db.Conversas.Where(x => x.Id == conversaId && x.EmpresaId == User.EmpresaId());
        if (User.IsInRole(ChatConstants.PerfilCliente)) { var clienteId = User.ClienteId()!.Value; return query.Where(x => x.ClienteId == clienteId); }
        if (User.IsInRole(ChatConstants.PerfilAtendente)) { var usuarioId = User.UsuarioId()!.Value; return query.Where(x => x.AtendenteId == usuarioId || x.AtendenteId == null && x.Setor!.UsuarioSetores.Any(us => us.UsuarioId == usuarioId)); }
        return query;
    }
}
