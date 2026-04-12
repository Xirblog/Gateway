using AuthService.Presentation.Grpc.Protos;
using BlogGateway.Presentation.Rest.Auth.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using static AuthService.Presentation.Grpc.Protos.Auth;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace BlogGateway.Presentation.Rest.Auth.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private const string AccessTokenCookie = "access_token";
    private const string RefreshTokenCookie = "refresh_token";
    private const string RefreshTokenPath = "/auth/refresh";

    private readonly AuthClient _authClient;

    public AuthController(AuthClient authClient)
    {
        _authClient = authClient;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model, CancellationToken cancellationToken)
    {
        RegisterResponse response = await _authClient.RegisterAsync(
            new RegisterRequest
            {
                Username = model.Username,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Age = model.Age,
            },
            new CallOptions(cancellationToken: cancellationToken));

        AppendTokenCookies(response.AccessToken, response.RefreshToken);

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model, CancellationToken cancellationToken)
    {
        try
        {
            LoginResponse response = await _authClient.LoginAsync(
                new LoginRequest
                {
                    Username = model.Username,
                    Password = model.Password,
                },
                new CallOptions(cancellationToken: cancellationToken));

            AppendTokenCookies(response.AccessToken, response.RefreshToken);

            return NoContent();
        }
        catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.Unauthenticated)
        {
            return Unauthorized();
        }
    }

    private void AppendTokenCookies(string accessToken, string refreshToken)
    {
        var accessOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        };

        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = RefreshTokenPath,
        };

        Response.Cookies.Append(AccessTokenCookie, accessToken, accessOptions);
        Response.Cookies.Append(RefreshTokenCookie, refreshToken, refreshOptions);
    }
}