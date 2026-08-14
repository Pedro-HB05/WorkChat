namespace WorkChat.Models;

public sealed class TransferenciaConversa
{
    public Guid Id { get; set; }
    public Guid ConversaId { get; set; }
    public Guid? SetorOrigemId { get; set; }
    public Guid? SetorDestinoId { get; set; }
    public Guid? AtendenteOrigemId { get; set; }
    public Guid? AtendenteDestinoId { get; set; }
    public string? Motivo { get; set; }
    public DateTime DataTransferencia { get; set; }
    public Conversa? Conversa { get; set; }
}
