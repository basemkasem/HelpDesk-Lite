namespace HelpDeskLite.Domain.Entities;

public class KnowledgeBaseArticle
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsPublished { get; set; } = true;
}
