using GitCodeSearch.Model;

namespace GitCodeSearch.Search
{
    public abstract record SearchQuery(Branch Branch, Repository Repository, bool IsCaseSensitive, bool IsRegex) 
        : RepositoryBranch(Repository, Branch);

    public record FileContentSearchQuery(string Expression, string Pattern, Branch Branch, Repository Repository, bool IsCaseSensitive, bool IsRegex)
        : SearchQuery(Branch, Repository, IsCaseSensitive, IsRegex);

    public record CommitMessageSearchQuery(string Expression, Branch Branch, Repository Repository, bool IsCaseSensitive, bool IsRegex)
        : SearchQuery(Branch, Repository, IsCaseSensitive, IsRegex);

}
