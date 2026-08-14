using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using WorkChat.Authentication;
using WorkChat.Data;
using WorkChat.Hubs;
using WorkChat.Middleware;
using WorkChat.Models;
using WorkChat.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddDataProtection()
        .UseEphemeralDataProtectionProvider();
}

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers(options =>
    options.Filters.Add(new ProducesAttribute("application/json")));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Informe o token JWT obtido em /api/auth/login."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
        });
});

builder.Services.AddDbContext<WorkChatDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(
        x => !string.IsNullOrWhiteSpace(x.Issuer),
        "Jwt:Issuer e obrigatorio.")
    .Validate(
        x => !string.IsNullOrWhiteSpace(x.Audience),
        "Jwt:Audience e obrigatorio.")
    .Validate(
        x => x.Key.Length >= 32,
        "Jwt:Key deve ter pelo menos 32 caracteres.")
    .ValidateOnStart();

var jwt =
    builder.Configuration
        .GetSection(JwtOptions.SectionName)
        .Get<JwtOptions>()
    ?? new();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(token) &&
                    context.HttpContext.Request.Path
                        .StartsWithSegments("/hubs/chat"))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.Administrador,
        p => p.RequireRole(ChatConstants.PerfilAdmin));

    options.AddPolicy(
        AuthorizationPolicies.Equipe,
        p => p.RequireRole(
            ChatConstants.PerfilAdmin,
            ChatConstants.PerfilAtendente));

    options.AddPolicy(
        AuthorizationPolicies.Chat,
        p => p.RequireRole(
            ChatConstants.PerfilAdmin,
            ChatConstants.PerfilAtendente,
            ChatConstants.PerfilCliente));

    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

builder.Services.AddRateLimiter(options =>
{
    var permitLimit =
        builder.Configuration.GetValue(
            "RateLimit:PermitLimit",
            120);

    var window =
        builder.Configuration.GetValue(
            "RateLimit:WindowSeconds",
            60);

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.User.FindFirstValue("empresa_id")
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(window),
                        QueueLimit = 0
                    }));

    options.OnRejected = async (context, ct) =>
        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = 429,
                Title = "Muitas requisicoes",
                Detail = "Tente novamente em instantes."
            },
            ct);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSignalR();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<PasswordHashService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<DistribuicaoService>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "WorkChat API v1");

    options.RoutePrefix = "swagger";
});

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
