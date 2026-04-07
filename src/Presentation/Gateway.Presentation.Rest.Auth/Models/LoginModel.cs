namespace Gateway.Presentation.Rest.Auth.Models;

public sealed class LoginModel
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
