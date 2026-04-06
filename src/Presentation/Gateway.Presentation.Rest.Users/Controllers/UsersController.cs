using Gateway.Presentation.Rest.Users.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using UserService.Presentation.Grpc.Protos;
using static UserService.Presentation.Grpc.Protos.UserService;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace Gateway.Presentation.Rest.Users.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly UserServiceClient _userServiceClient;

    public UsersController(UserServiceClient userServiceClient)
    {
        _userServiceClient = userServiceClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model, CancellationToken cancellationToken)
    {
        CreateUserResponse response = await _userServiceClient.CreateUserAsync(
            new CreateUserRequest
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Age = model.Age,
            },
            new CallOptions(cancellationToken: cancellationToken));

        return CreatedAtAction(
            nameof(FindUserById),
            new { id = response.UserId },
            new UserDto(
                response.UserId,
                response.FirstName,
                response.LastName,
                response.Age));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> FindUserById(string id, CancellationToken cancellationToken)
    {
        try
        {
            FindUserByIdResponse response = await _userServiceClient.FindUserByIdAsync(
                new FindUserByIdRequest { UserId = id },
                new CallOptions(cancellationToken: cancellationToken));

            return Ok(new UserDto(
                response.UserId,
                response.FirstName,
                response.LastName,
                response.Age));
        }
        catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
        {
            return NotFound();
        }
    }
}