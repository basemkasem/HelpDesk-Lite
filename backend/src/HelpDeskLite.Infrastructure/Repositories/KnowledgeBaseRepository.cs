using HelpDeskLite.Application.Interfaces;
using HelpDeskLite.Domain.Entities;
using HelpDeskLite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskLite.Infrastructure.Repositories;

public class KnowledgeBaseRepository(AppDbContext context) : IKnowledgeBaseRepository
{
    public async Task<IReadOnlyList<KnowledgeBaseArticle>> SearchAsync(string query, int limit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var terms = query.ToLowerInvariant()
            .Split([' ', ',', '.', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .Take(10)
            .ToList();

        if (terms.Count == 0)
        {
            return [];
        }

        var articles = await context.KnowledgeBaseArticles.AsNoTracking()
            .Where(a => a.IsPublished)
            .ToListAsync(cancellationToken);

        return articles
            .Select(a => new { Article = a, Score = ScoreArticle(a, terms) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Select(x => x.Article)
            .ToList();
    }

    private static int ScoreArticle(KnowledgeBaseArticle article, List<string> terms)
    {
        var haystack = $"{article.Title} {article.Summary} {article.Keywords}".ToLowerInvariant();
        return terms.Count(term => haystack.Contains(term, StringComparison.Ordinal));
    }
}
