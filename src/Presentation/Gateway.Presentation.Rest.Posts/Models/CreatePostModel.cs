namespace Gateway.Presentation.Rest.Posts.Models;

public sealed class CreatePostModel
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string MarkdownContent { get; set; } = string.Empty;

    public string AuthorId { get; set; } = string.Empty;
}
