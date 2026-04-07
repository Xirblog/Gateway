using System;

namespace Gateway.Presentation.Rest.Posts.Models;

public sealed record PostDto(
    string PostId,
    string Name,
    string Description,
    string MarkdownContent,
    string AuthorId,
    DateTime CreatedAt,
    DateTime UpdatedAt);