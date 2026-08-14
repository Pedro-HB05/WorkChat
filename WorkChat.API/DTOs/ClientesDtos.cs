using System.ComponentModel.DataAnnotations;

namespace WorkChat.DTOs;

public sealed record ClienteResponse(Guid Id, Guid EmpresaId, string Nome, string? Email, string? Telefone, bool Vip, DateTime DataCriacao);
public sealed record CreateClienteResponse(ClienteResponse Cliente, string AccessToken);
public sealed record CreateClienteRequest(Guid EmpresaId, [Required, MaxLength(150)] string Nome, [EmailAddress, MaxLength(150)] string? Email, [MaxLength(30)] string? Telefone);
public sealed record UpdateClienteRequest([Required, MaxLength(150)] string Nome, [EmailAddress, MaxLength(150)] string? Email, [MaxLength(30)] string? Telefone, bool Vip);
