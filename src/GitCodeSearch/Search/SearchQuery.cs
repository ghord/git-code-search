using GitCodeSearch.Model;

namespace GitCodeSearch.Search
{
    public abstract record SearchQuery(string? Branch, GitRepository Repository, bool IsCaseSensitive, bool IsRegex)
    {
    }

    public record FileContentSearchQuery(string Expression, string Pattern, string? Branch, GitRepository Repository, bool IsCaseSensitive, bool IsRegex)
        : SearchQuery(Branch, Repository, IsCaseSensitive, IsRegex);

    public record CommitMessageSearchQuery(string Expression, string? Branch, GitRepository Repository, bool IsCaseSensitive, bool IsRegex)
        : SearchQuery(Branch, Repository, IsCaseSensitive, IsRegex);

}
