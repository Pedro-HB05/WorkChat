namespace WorkChat.Models;

public sealed class Setor
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    public Empresa? Empresa { get; set; }
    public ICollection<UsuarioSetor> UsuarioSetores { get; set; } = [];
    public ICollection<Conversa> Conversas { get; set; } = [];
}
