namespace WorkChat.Models;

public sealed class Conversa
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid? SetorId { get; set; }
    public Guid? AtendenteId { get; set; }
    public string Status { get; set; } = ChatConstants.StatusAguardando;
    public int? PosicaoFila { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataEncerramento { get; set; }
    public int Prioridade { get; set; }
    public DateTime? DataInicioAtendimento { get; set; }

    public Empresa? Empresa { get; set; }
    public Cliente? Cliente { get; set; }
    public Setor? Setor { get; set; }
    public Usuario? Atendente { get; set; }
    public ICollection<Mensagem> Mensagens { get; set; } = [];
    public ICollection<TransferenciaConversa> Transferencias { get; set; } = [];
}
