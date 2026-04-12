namespace BlogGateway.Presentation.Rest.Posts.Models;

public sealed class UpdatePostModel
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? MarkdownContent { get; set; }
}