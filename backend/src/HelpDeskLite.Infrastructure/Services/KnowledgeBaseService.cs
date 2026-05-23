using HelpDeskLite.Application.DTOs.KnowledgeBase;
using HelpDeskLite.Application.Interfaces;

namespace HelpDeskLite.Infrastructure.Services;

public class KnowledgeBaseService(IKnowledgeBaseRepository repository) : IKnowledgeBaseService
{
    public async Task<IReadOnlyList<KnowledgeBaseSuggestionDto>> GetSuggestionsAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        var articles = await repository.SearchAsync(description, limit: 5, cancellationToken);
        return articles.Select((a, index) => new KnowledgeBaseSuggestionDto
        {
            Id = a.Id,
            Title = a.Title,
            Summary = a.Summary,
            Url = a.Url,
            RelevanceScore = 5 - index
        }).ToList();
    }
}
