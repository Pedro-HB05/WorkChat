using System.ComponentModel.DataAnnotations;
namespace WorkChat.DTOs;

public sealed record SetorResponse(Guid Id, Guid EmpresaId, string Nome, bool Ativo);
public sealed record CreateSetorRequest([Required, MaxLength(100)] string Nome);
public sealed record UpdateSetorRequest([Required, MaxLength(100)] string Nome, bool Ativo);
