namespace WorkChat.Models;

public sealed class Mensagem
{
    public Guid Id { get; set; }
    public Guid ConversaId { get; set; }
    public string RemetenteTipo { get; set; } = string.Empty;
    public Guid? UsuarioId { get; set; }
    public Guid? ClienteId { get; set; }
    public string Tipo { get; set; } = "Text";
    public string Conteudo { get; set; } = string.Empty;
    public DateTime DataEnvio { get; set; }

    public Conversa? Conversa { get; set; }
    public Usuario? Usuario { get; set; }
    public Cliente? Cliente { get; set; }
}
