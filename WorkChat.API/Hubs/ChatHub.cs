using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WorkChat.Authentication;
using WorkChat.Data;
using WorkChat.Models;

namespace WorkChat.Hubs;

[Authorize(Policy = AuthorizationPolicies.Chat)]
public sealed class ChatHub(WorkChatDbContext db) : Hub
{
    public async Task EntrarNaConversa(Guid conversaId)
    {
        if (!await PodeAcessar(conversaId)) throw new HubException("Conversa nao encontrada ou acesso negado.");
        await Groups.AddToGroupAsync(Context.ConnectionId, GrupoConversa(conversaId));
    }

    public Task SairDaConversa(Guid conversaId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GrupoConversa(conversaId));
    public static string GrupoConversa(Guid conversaId) => $"conversa:{conversaId}";

    private Task<bool> PodeAcessar(Guid conversaId)
    {
        var user = Context.User!;
        var query = db.Conversas.Where(x => x.Id == conversaId && x.EmpresaId == user.EmpresaId());
        if (user.IsInRole(ChatConstants.PerfilCliente)) { var clienteId = user.ClienteId()!.Value; query = query.Where(x => x.ClienteId == clienteId); }
        else if (user.IsInRole(ChatConstants.PerfilAtendente)) { var usuarioId = user.UsuarioId()!.Value; query = query.Where(x => x.AtendenteId == usuarioId || x.AtendenteId == null && x.Setor!.UsuarioSetores.Any(us => us.UsuarioId == usuarioId)); }
        return query.AnyAsync();
    }
}
