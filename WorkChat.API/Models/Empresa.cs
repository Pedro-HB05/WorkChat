namespace WorkChat.Models;

public sealed class Empresa
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; } = true;
    public string MensagemBoasVindas { get; set; } = "Ola! Como podemos ajudar?";
    public string MensagemEspera { get; set; } = "Aguarde, em breve um atendente estara disponivel.";
    public string? MensagemForaHorario { get; set; }
    public int LimiteChatsPadrao { get; set; } = 5;
    public DateTime DataCriacao { get; set; }
    public ICollection<Usuario> Usuarios { get; set; } = [];
    public ICollection<Cliente> Clientes { get; set; } = [];
    public ICollection<Setor> Setores { get; set; } = [];
    public ICollection<Conversa> Conversas { get; set; } = [];
}
