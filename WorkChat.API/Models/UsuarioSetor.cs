namespace WorkChat.Models;

public sealed class UsuarioSetor
{
    public Guid UsuarioId { get; set; }
    public Guid SetorId { get; set; }

    public Usuario? Usuario { get; set; }
    public Setor? Setor { get; set; }
}
