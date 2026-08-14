using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkChat.Authentication;
using WorkChat.Models;

namespace WorkChat.Services;

public sealed class TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    private readonly JwtOptions _options = options.Value;

    public string CriarParaUsuario(Usuario usuario) => Criar([
        new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new("empresa_id", usuario.EmpresaId.ToString()),
        new(ClaimTypes.Name, usuario.Nome),
        new(ClaimTypes.Email, usuario.Email),
        new(ClaimTypes.Role, usuario.Perfil)
    ]);

    public string CriarParaCliente(Cliente cliente) => Criar([
        new("cliente_id", cliente.Id.ToString()),
        new("empresa_id", cliente.EmpresaId.ToString()),
        new(ClaimTypes.Name, cliente.Nome),
        new(ClaimTypes.Role, ChatConstants.PerfilCliente)
    ]);

    private string Criar(IEnumerable<Claim> claims)
    {
        var agora = timeProvider.GetUtcNow().UtcDateTime;
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, agora, agora.AddMinutes(_options.ExpirationMinutes), credenciais);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
