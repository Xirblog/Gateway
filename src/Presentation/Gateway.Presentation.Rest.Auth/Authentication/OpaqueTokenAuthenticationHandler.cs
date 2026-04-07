using AuthService.Presentation.Grpc.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static AuthService.Presentation.Grpc.Protos.Auth;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace Gateway.Presentation.Rest.Auth.Authentication;

public sealed class OpaqueTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string BearerPrefix = "Bearer ";

    private readonly AuthClient _authClient;

    public OpaqueTokenAuthenticationHandler(
        AuthClient authClient,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _authClient = authClient;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = null;

        string? authorizationHeader = Request.Headers[HeaderNames.Authorization];

        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith(BearerPrefix, System.StringComparison.OrdinalIgnoreCase))
        {
            token = authorizationHeader[BearerPrefix.Length..].Trim();
        }
        else if (Request.Cookies.TryGetValue("access_token", out string? cookieToken))
        {
            token = cookieToken;
        }

        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.NoResult();
        }

        ValidateTokenResponse response;

        try
        {
            response = await _authClient.ValidateTokenAsync(
                new ValidateTokenRequest { AccessToken = token },
                new CallOptions(cancellationToken: Context.RequestAborted));
        }
        catch (RpcException ex) when (ex.StatusCode is GrpcStatusCode.Unauthenticated or GrpcStatusCode.NotFound)
        {
            return AuthenticateResult.Fail("Invalid token.");
        }

        if (!response.IsValid)
        {
            return AuthenticateResult.Fail("Invalid token.");
        }

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, response.UserId)
        ];

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
