using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkChat.Authentication;
using WorkChat.Data;
using WorkChat.DTOs;
using WorkChat.Models;

namespace WorkChat.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.Chat)]
[Route("api/setores")]
public sealed class SetoresController(WorkChatDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SetorResponse>>> Listar(CancellationToken ct) => Ok(await db.Setores.AsNoTracking()
        .Where(x => x.EmpresaId == User.EmpresaId() && (User.IsInRole(ChatConstants.PerfilAdmin) || x.Ativo))
        .OrderBy(x => x.Nome).Select(x => new SetorResponse(x.Id, x.EmpresaId, x.Nome, x.Ativo)).ToListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SetorResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var x = await db.Setores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == User.EmpresaId(), ct);
        return x is null ? NotFound() : Ok(new SetorResponse(x.Id, x.EmpresaId, x.Nome, x.Ativo));
    }

    [Authorize(Policy = AuthorizationPolicies.Administrador)]
    [HttpPost]
    public async Task<ActionResult<SetorResponse>> Criar(CreateSetorRequest request, CancellationToken ct)
    {
        var empresaId = User.EmpresaId(); var nome = request.Nome.Trim();
        if (await db.Setores.AnyAsync(x => x.EmpresaId == empresaId && x.Nome == nome, ct)) return Conflict("Ja existe um setor com este nome.");
        var x = new Setor { EmpresaId = empresaId, Nome = nome }; db.Add(x); await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = x.Id }, new SetorResponse(x.Id, x.EmpresaId, x.Nome, x.Ativo));
    }

    [Authorize(Policy = AuthorizationPolicies.Administrador)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, UpdateSetorRequest request, CancellationToken ct)
    {
        var x = await db.Setores.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == User.EmpresaId(), ct); if (x is null) return NotFound();
        var nome = request.Nome.Trim();
        if (await db.Setores.AnyAsync(s => s.EmpresaId == x.EmpresaId && s.Nome == nome && s.Id != id, ct)) return Conflict("Ja existe um setor com este nome.");
        x.Nome = nome; x.Ativo = request.Ativo; await db.SaveChangesAsync(ct); return NoContent();
    }
}
