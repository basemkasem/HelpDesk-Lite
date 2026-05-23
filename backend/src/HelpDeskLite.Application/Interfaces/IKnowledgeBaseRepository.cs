using HelpDeskLite.Domain.Entities;

namespace HelpDeskLite.Application.Interfaces;

public interface IKnowledgeBaseRepository
{
    Task<IReadOnlyList<KnowledgeBaseArticle>> SearchAsync(string query, int limit, CancellationToken cancellationToken = default);
}
