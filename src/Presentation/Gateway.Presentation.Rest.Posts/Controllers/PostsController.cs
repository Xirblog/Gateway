using Gateway.Presentation.Rest.Posts.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostService.Presentation.Grpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static PostService.Presentation.Grpc.Protos.PostService;
using GrpcStatusCode = Grpc.Core.StatusCode;

namespace Gateway.Presentation.Rest.Posts.Controllers;

[Authorize]
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

        return Created(string.Empty, new PostDto(response.PostId, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue));
    }

    [HttpGet]
    public async Task<IActionResult> QueryPosts([FromQuery] QueryPostsModel model, CancellationToken cancellationToken)
    {
        var request = new QueryPostsRequest();

        if (model.PostIds is { Count: > 0 })
        {
            request.PostIds.AddRange(model.PostIds);
        }

        if (model.NameSubstring is not null)
        {
            request.NameSubstring = model.NameSubstring;
        }

        if (model.DescriptionSubstring is not null)
        {
            request.DescriptionSubstring = model.DescriptionSubstring;
        }

        if (model.MarkdownContentSubstring is not null)
        {
            request.MarkdownContentSubstring = model.MarkdownContentSubstring;
        }

        if (model.AuthorIds is { Count: > 0 })
        {
            request.AuthorIds.AddRange(model.AuthorIds);
        }

        if (model.CreatedBefore is not null)
        {
            request.CreatedBefore = Timestamp.FromDateTime(model.CreatedBefore.Value.ToUniversalTime());
        }

        if (model.CreatedAfter is not null)
        {
            request.CreatedAfter = Timestamp.FromDateTime(model.CreatedAfter.Value.ToUniversalTime());
        }

        if (model.UpdatedBefore is not null)
        {
            request.UpdatedBefore = Timestamp.FromDateTime(model.UpdatedBefore.Value.ToUniversalTime());
        }

        if (model.UpdatedAfter is not null)
        {
            request.UpdatedAfter = Timestamp.FromDateTime(model.UpdatedAfter.Value.ToUniversalTime());
        }

        QueryPostsResponse response = await _postServiceClient.QueryPostsAsync(
            request,
            new CallOptions(cancellationToken: cancellationToken));

        IEnumerable<PostDto> posts = response.Posts.Select(p => new PostDto(
            p.PostId,
            p.Name,
            p.Description,
            p.MarkdownContent,
            p.AuthorId,
            p.CreatedAt.ToDateTime(),
            p.UpdatedAt.ToDateTime()));

        return Ok(posts);
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