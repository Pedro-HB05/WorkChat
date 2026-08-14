using System.ComponentModel.DataAnnotations;

namespace WorkChat.DTOs;

public sealed record MensagemResponse(
    Guid Id,
    Guid ConversaId,
    string RemetenteTipo,
    Guid? UsuarioId,
    Guid? ClienteId,
    string Conteudo,
    DateTime DataEnvio);

public sealed record EnviarMensagemRequest(
    [Required, MinLength(1), MaxLength(10000)] string Conteudo);
