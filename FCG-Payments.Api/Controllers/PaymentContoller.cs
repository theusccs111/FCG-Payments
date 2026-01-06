using FCG_Payments.Application.Shared.Interfaces;
using FCG_Payments.Application.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FCG_Payments.Api.Controllers
{
    [ApiController]
    [Route("payments")]
    public class PaymentContoller(IPaymentService service) : ControllerBase
    {
        /// <summary>
        /// Efetua um pagamento.
        /// </summary>
        /// <param name="paymentId">Código do pagamento</param>
        /// <param name="cancellationToken">Token que monitora o cancelamento do processo.</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        [HttpPost]
        public async Task<IResult> PayOrderAsync([FromBody] Guid paymentId, CancellationToken cancellationToken = default)
        {
            var result = await service.PayAsync(paymentId, cancellationToken);

            if(result.IsFailure)
            {
                return result.Error.Code switch
                {
                    "404" => TypedResults.NotFound(new Error("404", result.Error.Message)),
                    "402" => TypedResults.StatusCode(StatusCodes.Status402PaymentRequired),
                    _ => TypedResults.BadRequest(new Error("400", result.Error.Message))
                };
            }

            return TypedResults.Ok();
        }

        /// <summary>
        /// Debug - Verifica autenticação e claims
        /// </summary>
        [HttpGet("/debug-auth")]
        public IActionResult DebugAuth()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var apimUserId = Request.Headers["X-User-Id"].ToString();
            var apimEmail = Request.Headers["X-User-Email"].ToString();
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            }).ToList();

            return Ok(new
            {
                // Headers recebidos
                Headers = new
                {
                    AuthorizationHeader = string.IsNullOrEmpty(authHeader) ? "Ausente" : $"Presente ({authHeader.Substring(0, Math.Min(50, authHeader.Length))}...)",
                    XUserId = string.IsNullOrEmpty(apimUserId) ? "Ausente" : apimUserId,
                    XUserEmail = string.IsNullOrEmpty(apimEmail) ? "Ausente" : apimEmail
                },

                // Estado de autenticação
                Authentication = new
                {
                    IsAuthenticated = isAuthenticated,
                    AuthenticationType = User.Identity?.AuthenticationType,
                    IdentityName = User.Identity?.Name
                },

                // Claims do usuário
                Claims = claims,

                // Claims específicas (se houver)
                SpecificClaims = new
                {
                    UserId = User.FindFirst("UserId")?.Value ?? "Não encontrado",
                    Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "Não encontrado",
                    Name = User.FindFirst("Name")?.Value ?? "Não encontrado",
                    Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Não encontrado"
                }
            });
        }
    }
}
