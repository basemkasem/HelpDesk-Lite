using HelpDeskLite.Application.DTOs.KnowledgeBase;

namespace HelpDeskLite.Application.Interfaces;

public interface IKnowledgeBaseService
{
    Task<IReadOnlyList<KnowledgeBaseSuggestionDto>> GetSuggestionsAsync(string description, CancellationToken cancellationToken = default);
}
