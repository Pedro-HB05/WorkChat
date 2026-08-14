using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using WorkChat.Data;
using WorkChat.Models;

namespace WorkChat.Services;

public sealed class DistribuicaoService(WorkChatDbContext db, ILogger<DistribuicaoService> logger)
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public async Task<bool> TentarDistribuirAsync(Conversa conversa, CancellationToken ct)
    {
        var gate = Locks.GetOrAdd($"{conversa.EmpresaId}:{conversa.SetorId}", _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try { return await TentarDistribuirCoreAsync(conversa, ct); }
        finally { gate.Release(); }
    }

    private async Task<bool> TentarDistribuirCoreAsync(Conversa conversa, CancellationToken ct)
    {
        var candidato = await db.Usuarios
            .Where(u => u.EmpresaId == conversa.EmpresaId && u.Ativo && u.Perfil == ChatConstants.PerfilAtendente)
            .Where(u => u.StatusAtendimento == ChatConstants.PresencaOnline || u.StatusAtendimento == ChatConstants.PresencaOcupado)
            .Where(u => u.UsuarioSetores.Any(us => us.SetorId == conversa.SetorId))
            .Select(u => new
            {
                Usuario = u,
                Chats = db.Conversas.Count(c => c.AtendenteId == u.Id && c.Status == ChatConstants.StatusEmAtendimento)
            })
            .Where(x => x.Chats < x.Usuario.LimiteChats)
            .OrderBy(x => x.Chats)
            .ThenBy(x => x.Usuario.UltimaDistribuicao ?? DateTime.MinValue)
            .Select(x => x.Usuario)
            .FirstOrDefaultAsync(ct);

        if (candidato is null)
        {
            logger.LogInformation("Conversa do setor {SetorId} permaneceu na fila por falta de capacidade", conversa.SetorId);
            return false;
        }
        conversa.AtendenteId = candidato.Id;
        conversa.Status = ChatConstants.StatusEmAtendimento;
        conversa.DataInicioAtendimento ??= DateTime.UtcNow;
        candidato.UltimaDistribuicao = DateTime.UtcNow;
        logger.LogInformation("Conversa distribuida para o atendente {AtendenteId} no setor {SetorId}", candidato.Id, conversa.SetorId);
        return true;
    }

    public async Task DistribuirProximoAsync(Guid empresaId, Guid setorId, CancellationToken ct)
    {
        var gate = Locks.GetOrAdd($"{empresaId}:{setorId}", _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            var proxima = await db.Conversas
                .Where(c => c.EmpresaId == empresaId && c.SetorId == setorId && c.Status == ChatConstants.StatusAguardando)
                .OrderByDescending(c => c.Prioridade).ThenBy(c => c.DataAbertura)
                .FirstOrDefaultAsync(ct);
            if (proxima is not null) await TentarDistribuirCoreAsync(proxima, ct);
        }
        finally { gate.Release(); }
    }
}
