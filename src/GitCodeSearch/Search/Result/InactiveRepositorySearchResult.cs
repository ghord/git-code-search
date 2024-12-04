using GitCodeSearch.Model;

namespace GitCodeSearch.Search.Result;

public record InactiveRepositorySearchResult(Repository Repository, int Count) : ISearchResult
{
    public override string ToString() => $"{Count} result{(Count == 1 ? "" : "s")} (inactive)";
}
