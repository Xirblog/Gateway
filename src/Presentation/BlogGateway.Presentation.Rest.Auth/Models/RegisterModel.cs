namespace BlogGateway.Presentation.Rest.Auth.Models;

public sealed class RegisterModel
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public long Age { get; set; } = 0;
}
