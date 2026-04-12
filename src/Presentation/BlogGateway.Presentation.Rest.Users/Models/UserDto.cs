namespace BlogGateway.Presentation.Rest.Users.Models;

public sealed record UserDto(
    string UserId,
    string FirstName,
    string LastName,
    long Age);