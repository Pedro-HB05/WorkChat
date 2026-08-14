namespace WorkChat.Models;

public sealed class Usuario
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; }
    public string StatusAtendimento { get; set; } = ChatConstants.PresencaOffline;
    public int LimiteChats { get; set; } = 5;
    public DateTime? UltimaDistribuicao { get; set; }

    public Empresa? Empresa { get; set; }
    public ICollection<UsuarioSetor> UsuarioSetores { get; set; } = [];
    public ICollection<Conversa> ConversasAtendidas { get; set; } = [];
    public ICollection<Mensagem> Mensagens { get; set; } = [];
    public ICollection<TransferenciaConversa> TransferenciasSolicitadas { get; set; } = [];
}
