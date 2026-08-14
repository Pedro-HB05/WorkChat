using System.ComponentModel.DataAnnotations;

namespace WorkChat.DTOs;

public sealed record LoginRequest(
    [Required, MaxLength(150)] string EmpresaNome,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MaxLength(100)] string Senha);

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAt, UsuarioResponse Usuario);
