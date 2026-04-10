using Gateway.Presentation.Rest.Users.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using UserService.Presentation.Grpc.Protos;
using static UserService.Presentation.Grpc.Protos.UserService;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace Gateway.Presentation.Rest.Users.Controllers;

[Authorize]
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

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return NotFound("User ID claim not found in the token.");
        }

        try
        {
            FindUserByIdResponse response = await _userServiceClient.FindUserByIdAsync(
                new FindUserByIdRequest { UserId = userId },
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

    [HttpGet("{userId}")]
    public async Task<IActionResult> FindUserById(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            FindUserByIdResponse response = await _userServiceClient.FindUserByIdAsync(
                new FindUserByIdRequest { UserId = userId.ToString() },
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