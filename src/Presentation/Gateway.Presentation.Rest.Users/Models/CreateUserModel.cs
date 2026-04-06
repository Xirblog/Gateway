namespace Gateway.Presentation.Rest.Users.Models;

public sealed class CreateUserModel
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public long Age { get; set; } = 0;
}