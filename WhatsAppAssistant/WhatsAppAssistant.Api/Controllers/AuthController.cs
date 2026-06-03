using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Mvc;
using WhatsAppAssistant.Core.Interfaces;

namespace WhatsAppAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public AuthController(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    [HttpGet("google")]
    public IActionResult GoogleAuth([FromQuery] string numero)
    {
        var clientId = _configuration["Google:ClientId"] ?? "";
        var redirectUri = _configuration["Google:RedirectUri"] ?? "";

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = _configuration["Google:ClientSecret"] ?? "" },
            Scopes = new[] { "https://www.googleapis.com/auth/calendar" }
        });

        var authUrl = flow.CreateAuthorizationCodeRequest(redirectUri);
        authUrl.State = numero;

        return Redirect(authUrl.Build().ToString());
    }

    [HttpGet("callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        var clientId = _configuration["Google:ClientId"] ?? "";
        var clientSecret = _configuration["Google:ClientSecret"] ?? "";
        var redirectUri = _configuration["Google:RedirectUri"] ?? "";

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            Scopes = new[] { "https://www.googleapis.com/auth/calendar" }
        });

        var token = await flow.ExchangeCodeForTokenAsync("user", code, redirectUri, CancellationToken.None);

        await _usuarioRepository.SaveTokensAsync(
            state,
            token.AccessToken,
            token.RefreshToken ?? "",
            ""
        );

        return Ok("Calendario autorizado correctamente. Ya puedes usar el asistente.");
    }
}