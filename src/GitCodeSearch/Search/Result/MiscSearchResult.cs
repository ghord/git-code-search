using GitCodeSearch.Model;

namespace GitCodeSearch.Search.Result;

public record InactiveRepositorySearchResult(Repository Repository, int Count) : ISearchResult
{
    public override string ToString() => $"{Count} result{(Count == 1 ? "" : "s")} (inactive)";
}

public record MissingBranchRepositorySearchResult(SearchQuery Query) : ISearchResult
{
    public override string ToString() => $"Missing branch {Query.Branch} {(Query.Repository.Active ? "" : "(inactive)")}";
}

public record ErrorSearchResult(SearchQuery Query, string Error) : ISearchResult
{
    public override string ToString() => $"Error: {Error}";
}
