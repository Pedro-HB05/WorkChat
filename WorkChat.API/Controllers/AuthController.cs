using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WorkChat.Authentication;
using WorkChat.Data;
using WorkChat.DTOs;
using WorkChat.Services;

namespace WorkChat.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    WorkChatDbContext db,
    PasswordHashService passwordHash,
    TokenService tokenService,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider,
    ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var empresa = request.EmpresaNome.Trim().ToLowerInvariant();
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.AsNoTracking()
            .Include(x => x.Empresa)
            .FirstOrDefaultAsync(x => x.Email == email && x.Empresa!.Nome.ToLower() == empresa && x.Ativo && x.Empresa.Ativa, ct);

        if (usuario is null || !passwordHash.Verify(request.Senha, usuario.SenhaHash))
        {
            logger.LogWarning("Falha de login para empresa {Empresa} e e-mail {Email}", empresa, email);
            return Unauthorized(new ProblemDetails { Title = "Credenciais invalidas", Status = StatusCodes.Status401Unauthorized });
        }

        var token = tokenService.CriarParaUsuario(usuario);
        var expiraEm = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(jwtOptions.Value.ExpirationMinutes);
        var response = new UsuarioResponse(usuario.Id, usuario.EmpresaId, usuario.Nome, usuario.Email, usuario.Perfil, usuario.Ativo, usuario.StatusAtendimento, usuario.LimiteChats, usuario.DataCriacao);
        return Ok(new LoginResponse(token, expiraEm, response));
    }
}
