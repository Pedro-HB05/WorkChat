using System.Security.Claims;

namespace WorkChat.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid EmpresaId(this ClaimsPrincipal principal) => ObterGuid(principal, "empresa_id");
    public static Guid? UsuarioId(this ClaimsPrincipal principal) => ObterGuidOpcional(principal, ClaimTypes.NameIdentifier);
    public static Guid? ClienteId(this ClaimsPrincipal principal) => ObterGuidOpcional(principal, "cliente_id");

    private static Guid ObterGuid(ClaimsPrincipal principal, string tipo) =>
        ObterGuidOpcional(principal, tipo) ?? throw new UnauthorizedAccessException($"Claim obrigatoria ausente: {tipo}.");

    private static Guid? ObterGuidOpcional(ClaimsPrincipal principal, string tipo) =>
        Guid.TryParse(principal.FindFirstValue(tipo), out var valor) ? valor : null;
}
