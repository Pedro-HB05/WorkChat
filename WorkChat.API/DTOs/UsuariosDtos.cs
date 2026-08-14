using System.ComponentModel.DataAnnotations;
namespace WorkChat.DTOs;

public sealed record UsuarioResponse(Guid Id, Guid EmpresaId, string Nome, string Email, string Perfil, bool Ativo, string StatusAtendimento, int LimiteChats, DateTime DataCriacao);
public sealed record CreateUsuarioRequest(Guid EmpresaId, [Required, MaxLength(150)] string Nome, [Required, EmailAddress, MaxLength(150)] string Email, [Required, MinLength(8), MaxLength(100)] string Senha, [Required, MaxLength(20)] string Perfil, [Range(1, 100)] int LimiteChats = 5);
public sealed record UpdateUsuarioRequest([Required, MaxLength(100)] string Nome, [Required, EmailAddress, MaxLength(255)] string Email, [Required, MaxLength(20)] string Perfil, bool Ativo, [Range(1, 100)] int LimiteChats = 5);
public sealed record AtualizarStatusUsuarioRequest([Required, MaxLength(20)] string Status);
public sealed record VincularUsuarioSetorRequest(Guid SetorId);
