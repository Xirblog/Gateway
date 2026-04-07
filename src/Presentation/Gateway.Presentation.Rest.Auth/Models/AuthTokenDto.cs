namespace Gateway.Presentation.Rest.Auth.Models;

public sealed record AuthTokenDto(string AccessToken, string RefreshToken);
