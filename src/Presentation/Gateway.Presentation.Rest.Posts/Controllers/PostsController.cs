using Gateway.Presentation.Rest.Posts.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using PostService.Presentation.Grpc.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;
using static PostService.Presentation.Grpc.Protos.PostService;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace Gateway.Presentation.Rest.Posts.Controllers;

[ApiController]
[Route("posts")]
public class PostsController : ControllerBase
{
    private readonly PostServiceClient _postServiceClient;

    public PostsController(PostServiceClient postServiceClient)
    {
        _postServiceClient = postServiceClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostModel model, CancellationToken cancellationToken)
    {
        CreatePostResponse response = await _postServiceClient.CreatePostAsync(
            new CreatePostRequest
            {
                Name = model.Name,
                Description = model.Description,
                MarkdownContent = model.MarkdownContent,
                AuthorId = model.AuthorId,
            },
            new CallOptions(cancellationToken: cancellationToken));

        return Created(string.Empty, new PostDto(response.PostId));
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdatePost(
        Guid userId,
        [FromBody] UpdatePostModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new UpdatePostRequest { PostId = userId.ToString() };

            if (model.Name is not null)
            {
                request.Name = model.Name;
            }

            if (model.Description is not null)
            {
                request.Description = model.Description;
            }

            if (model.MarkdownContent is not null)
            {
                request.MarkdownContent = model.MarkdownContent;
            }

            UpdatePostResponse response = await _postServiceClient.UpdatePostAsync(
                request,
                new CallOptions(cancellationToken: cancellationToken));

            return response.Success ? NoContent() : NotFound();
        }
        catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeletePost(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            DeletePostResponse response = await _postServiceClient.DeletePostAsync(
                new DeletePostRequest { PostId = userId.ToString() },
                new CallOptions(cancellationToken: cancellationToken));

            return response.Success ? NoContent() : NotFound();
        }
        catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
        {
            return NotFound();
        }
    }
}