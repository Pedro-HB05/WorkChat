using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkChat.Authentication;
using WorkChat.Data;
using WorkChat.DTOs;
using WorkChat.Models;

namespace WorkChat.Controllers;

[ApiController]
[Route("api/empresas")]
public sealed class EmpresasController(WorkChatDbContext db, ILogger<EmpresasController> logger) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.Administrador)]
    [HttpGet]
    public async Task<ActionResult<EmpresaResponse>> ObterAtual(CancellationToken ct)
    {
        var empresa = await db.Empresas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == User.EmpresaId(), ct);
        return empresa is null ? NotFound() : Ok(CriarResponse(empresa));
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<EmpresaResponse>> Criar(CreateEmpresaRequest request, CancellationToken ct)
    {
        var empresa = new Empresa
        {
            Nome = request.Nome.Trim(),
            MensagemBoasVindas = request.MensagemBoasVindas?.Trim() ?? "Olá! Como podemos ajudar?",
            MensagemEspera = request.MensagemEspera?.Trim() ?? "Aguarde, em breve um atendente estará disponível.",
            MensagemForaHorario = request.MensagemForaHorario?.Trim(),
            DataCriacao = DateTime.UtcNow
        };
        db.Empresas.Add(empresa);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Empresa {EmpresaId} criada", empresa.Id);
        return Created(string.Empty, CriarResponse(empresa));
    }

    [Authorize(Policy = AuthorizationPolicies.Administrador)]
    [HttpPut]
    public async Task<IActionResult> Atualizar(UpdateEmpresaRequest request, CancellationToken ct)
    {
        var empresa = await db.Empresas.FirstOrDefaultAsync(x => x.Id == User.EmpresaId(), ct);
        if (empresa is null) return NotFound();
        empresa.Nome = request.Nome.Trim();
        empresa.Ativa = request.Ativa;
        empresa.MensagemBoasVindas = request.MensagemBoasVindas.Trim();
        empresa.MensagemEspera = request.MensagemEspera.Trim();
        empresa.MensagemForaHorario = request.MensagemForaHorario?.Trim();
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static EmpresaResponse CriarResponse(Empresa x) =>
        new(x.Id, x.Nome, x.Ativa, x.MensagemBoasVindas, x.MensagemEspera, x.MensagemForaHorario, x.LimiteChatsPadrao, x.DataCriacao);
}
