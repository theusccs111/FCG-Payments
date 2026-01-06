using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FCG_Payments.Api
{
    public class ApimAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public ApimAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // ✅ APENAS processa se tem headers do APIM
                if (Request.Headers.TryGetValue("X-User-Id", out var userId) &&
                    !string.IsNullOrEmpty(userId))
                {
                    Logger.LogInformation($"APIM Auth: Processando usuário {userId}");

                    var claims = new List<Claim>
                    {
                        new Claim("UserId", userId.ToString()),
                        new Claim(ClaimTypes.Name, "APIM-Authenticated"),
                        new Claim("AuthenticationSource", "APIM")
                    };

                    if (Request.Headers.TryGetValue("X-User-Email", out var email) && !string.IsNullOrEmpty(email))
                    {
                        claims.Add(new Claim(ClaimTypes.Email, email.ToString()));
                    }

                    if (Request.Headers.TryGetValue("X-User-Role", out var role) && !string.IsNullOrEmpty(role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                    }

                    var identity = new ClaimsIdentity(claims, "APIM");
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, "APIM");

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }

                // ❌ NÃO TEM HEADERS DO APIM
                // Retorna Fail() para que o JWT Bearer handler tente processar
                Logger.LogInformation("APIM Auth: Sem headers APIM, deixando JWT handler processar");
                return Task.FromResult(AuthenticateResult.Fail("Sem headers APIM, tentando JWT"));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro na autenticação APIM");
                return Task.FromResult(AuthenticateResult.Fail("APIM authentication error: " + ex.Message));
            }
        }
    }
}