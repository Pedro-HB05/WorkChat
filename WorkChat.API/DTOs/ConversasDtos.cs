using System.ComponentModel.DataAnnotations;
namespace WorkChat.DTOs;

public sealed record ConversaResponse(Guid Id, Guid EmpresaId, Guid ClienteId, string? ClienteNome, string? ClienteEmail, Guid? SetorId, string? SetorNome, Guid? AtendenteId, string? AtendenteNome, string Status, int Prioridade, int? PosicaoFila, DateTime DataAbertura, DateTime? DataInicioAtendimento, DateTime? DataEncerramento);
public sealed record CreateConversaRequest(Guid SetorId, [Range(0, 100)] int Prioridade = 0);
public sealed record AssumirConversaRequest(Guid AtendenteId);
public sealed record TransferirConversaRequest(Guid SetorDestinoId, Guid? AtendenteDestinoId, [MaxLength(500)] string? Motivo);
public sealed record TransferenciaConversaResponse(Guid Id, Guid ConversaId, Guid? SetorOrigemId, Guid? SetorDestinoId, Guid? AtendenteOrigemId, Guid? AtendenteDestinoId, string? Motivo, DateTime DataTransferencia);
