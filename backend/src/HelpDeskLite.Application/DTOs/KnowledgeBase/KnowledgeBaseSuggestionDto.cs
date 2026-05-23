namespace HelpDeskLite.Application.DTOs.KnowledgeBase;

public class KnowledgeBaseSuggestionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Url { get; set; }
    public int RelevanceScore { get; set; }
}
