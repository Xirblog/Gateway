using System;
using System.Collections.Generic;

namespace BlogGateway.Presentation.Rest.Posts.Models;

public sealed class QueryPostsModel
{
    public IReadOnlyList<string>? PostIds { get; set; }

    public string? NameSubstring { get; set; }

    public string? DescriptionSubstring { get; set; }

    public string? MarkdownContentSubstring { get; set; }

    public IReadOnlyList<string>? AuthorIds { get; set; }

    public DateTime? CreatedBefore { get; set; }

    public DateTime? CreatedAfter { get; set; }

    public DateTime? UpdatedBefore { get; set; }

    public DateTime? UpdatedAfter { get; set; }
}