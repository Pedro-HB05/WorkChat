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
[Route("api/clientes")]
public sealed class ClientesController(WorkChatDbContext db, TokenService tokenService) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.Equipe)]
    [HttpGet]
    public async Task<ActionResult<PaginaResponse<ClienteResponse>>> Listar([FromQuery] PaginacaoQuery paginacao, CancellationToken ct)
    {
        var query = db.Clientes.AsNoTracking().Where(x => x.EmpresaId == User.EmpresaId()).OrderBy(x => x.Nome);
        var total = await query.CountAsync(ct);
        var itens = await query.Skip((paginacao.Pagina - 1) * paginacao.Tamanho).Take(paginacao.Tamanho)
            .Select(x => new ClienteResponse(x.Id, x.EmpresaId, x.Nome, x.Email, x.Telefone, x.Vip, x.DataCriacao)).ToListAsync(ct);
        return Ok(new PaginaResponse<ClienteResponse>(itens, paginacao.Pagina, paginacao.Tamanho, total));
    }

    [Authorize(Policy = AuthorizationPolicies.Equipe)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var x = await db.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == User.EmpresaId(), ct);
        return x is null ? NotFound() : Ok(new ClienteResponse(x.Id, x.EmpresaId, x.Nome, x.Email, x.Telefone, x.Vip, x.DataCriacao));
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<CreateClienteResponse>> Criar(CreateClienteRequest request, CancellationToken ct)
    {
        if (!await db.Empresas.AnyAsync(x => x.Id == request.EmpresaId && x.Ativa, ct)) return NotFound("Empresa ativa nao encontrada.");
        var email = request.Email?.Trim().ToLowerInvariant();
        if (email is not null && await db.Clientes.AnyAsync(x => x.EmpresaId == request.EmpresaId && x.Email == email, ct)) return Conflict("Ja existe um cliente com este e-mail.");

        var cliente = new Cliente { EmpresaId = request.EmpresaId, Nome = request.Nome.Trim(), Email = email, Telefone = request.Telefone?.Trim(), DataCriacao = DateTime.UtcNow };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct);
        var dto = new ClienteResponse(cliente.Id, cliente.EmpresaId, cliente.Nome, cliente.Email, cliente.Telefone, false, cliente.DataCriacao);
        return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, new CreateClienteResponse(dto, tokenService.CriarParaCliente(cliente)));
    }

    [Authorize(Policy = AuthorizationPolicies.Equipe)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, UpdateClienteRequest request, CancellationToken ct)
    {
        var item = await db.Clientes.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == User.EmpresaId(), ct);
        if (item is null) return NotFound();
        var email = request.Email?.Trim().ToLowerInvariant();
        if (email is not null && await db.Clientes.AnyAsync(x => x.EmpresaId == item.EmpresaId && x.Email == email && x.Id != id, ct)) return Conflict("E-mail ja esta em uso.");
        item.Nome = request.Nome.Trim(); item.Email = email; item.Telefone = request.Telefone?.Trim();
        item.Vip = request.Vip;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Equipe)]
    [HttpGet("{id:guid}/conversas")]
    public async Task<ActionResult<PaginaResponse<ConversaResponse>>> Historico(Guid id, [FromQuery] PaginacaoQuery paginacao, CancellationToken ct)
    {
        var query = db.Conversas.AsNoTracking().Where(x => x.ClienteId == id && x.EmpresaId == User.EmpresaId()).OrderByDescending(x => x.DataAbertura);
        var total = await query.CountAsync(ct);
        var itens = await query.Skip((paginacao.Pagina - 1) * paginacao.Tamanho).Take(paginacao.Tamanho)
            .Select(x => new ConversaResponse(x.Id, x.EmpresaId, x.ClienteId, x.Cliente!.Nome, x.Cliente.Email, x.SetorId, x.Setor!.Nome, x.AtendenteId, x.Atendente != null ? x.Atendente.Nome : null, x.Status, x.Prioridade, null, x.DataAbertura, x.DataInicioAtendimento, x.DataEncerramento)).ToListAsync(ct);
        return Ok(new PaginaResponse<ConversaResponse>(itens, paginacao.Pagina, paginacao.Tamanho, total));
    }
}
