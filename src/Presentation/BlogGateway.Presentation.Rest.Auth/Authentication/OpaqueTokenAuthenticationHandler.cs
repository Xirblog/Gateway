using AuthService.Presentation.Grpc.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static AuthService.Presentation.Grpc.Protos.Auth;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace BlogGateway.Presentation.Rest.Auth.Authentication;

public sealed class OpaqueTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string BearerPrefix = "Bearer ";
    private const string AccessTokenCookieName = "access_token";

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
        string tokenSource = "none";

        string? authorizationHeader = Request.Headers[HeaderNames.Authorization];

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            token = NormalizeToken(authorizationHeader);
            tokenSource = "authorization-header";
        }
        else if (Request.Cookies.TryGetValue(AccessTokenCookieName, out string? cookieToken))
        {
            token = NormalizeToken(cookieToken);
            tokenSource = "access-token-cookie";
        }

        if (string.IsNullOrEmpty(token))
        {
            Logger.LogDebug("No access token found in request.");
            return AuthenticateResult.NoResult();
        }

        Logger.LogDebug(
            "Validating opaque token from {TokenSource}. Length={TokenLength}, Fingerprint={TokenFingerprint}",
            tokenSource,
            token.Length,
            ComputeTokenFingerprint(token));

        ValidateTokenResponse response;

        try
        {
            response = await _authClient.ValidateTokenAsync(
                new ValidateTokenRequest { AccessToken = token },
                new CallOptions(cancellationToken: Context.RequestAborted));
        }
        catch (RpcException ex) when (ex.StatusCode is GrpcStatusCode.Unauthenticated or GrpcStatusCode.NotFound)
        {
            Logger.LogInformation(
                ex,
                "Token validation failed in auth service. Source={TokenSource}, Length={TokenLength}, Fingerprint={TokenFingerprint}",
                tokenSource,
                token.Length,
                ComputeTokenFingerprint(token));
            return AuthenticateResult.Fail("Invalid token.");
        }

        if (!response.IsValid)
        {
            Logger.LogInformation(
                "Auth service reported invalid token. Source={TokenSource}, Length={TokenLength}, Fingerprint={TokenFingerprint}",
                tokenSource,
                token.Length,
                ComputeTokenFingerprint(token));
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

    private static string? NormalizeToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();

        if (normalized.Length >= 2 &&
            ((normalized[0] == '"' && normalized[^1] == '"') ||
             (normalized[0] == '\'' && normalized[^1] == '\'')))
        {
            normalized = normalized[1..^1].Trim();
        }

        if (normalized.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[BearerPrefix.Length..].Trim();
        }

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string ComputeTokenFingerprint(string token)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash.AsSpan(0, 6));
    }
}
