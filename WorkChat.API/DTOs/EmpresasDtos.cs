using System.ComponentModel.DataAnnotations;
namespace WorkChat.DTOs;

public sealed record EmpresaResponse(Guid Id, string Nome, bool Ativa, string? MensagemBoasVindas, string? MensagemEspera, string? MensagemForaHorario, int LimiteChatsPadrao, DateTime DataCriacao);
public sealed record CreateEmpresaRequest([Required, MaxLength(150)] string Nome, [MaxLength(500)] string? MensagemBoasVindas, [MaxLength(500)] string? MensagemEspera, [MaxLength(500)] string? MensagemForaHorario);
public sealed record UpdateEmpresaRequest([Required, MaxLength(150)] string Nome, bool Ativa, [Required, MaxLength(500)] string MensagemBoasVindas, [Required, MaxLength(500)] string MensagemEspera, [MaxLength(500)] string? MensagemForaHorario);
