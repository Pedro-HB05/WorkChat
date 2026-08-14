namespace WorkChat.Models;

public sealed class Cliente
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public bool Vip { get; set; }
    public int Prioridade { get; set; }
    public DateTime DataCriacao { get; set; }

    public Empresa? Empresa { get; set; }
    public ICollection<Conversa> Conversas { get; set; } = [];
    public ICollection<Mensagem> Mensagens { get; set; } = [];
}
