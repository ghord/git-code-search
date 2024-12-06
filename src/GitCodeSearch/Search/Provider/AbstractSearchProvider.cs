using GitCodeSearch.Model;
using GitCodeSearch.Search.Result;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GitCodeSearch.Search.Provider;

public abstract class AbstractSearchProvider(SearchQuery query) : ISearchProvider
{
    public abstract Repository Repository { get; }
    public abstract IEnumerable<string> GetArguments();
    public abstract bool TryParseSearchResult(string text, [NotNullWhen(true)] out ISearchResult? searchResult);

    public bool TryParseErrorResult(string error, [NotNullWhen(true)] out ISearchResult? errorResult)
    {
        if (error.StartsWith("fatal: unable to resolve revision"))
        {
            errorResult = new MissingBranchRepositorySearchResult(query);
            return Settings.Current.WarnOnMissingBranch;
        }
        else if(string.IsNullOrEmpty(error))
        {
            errorResult = null;
            return false;
        }
        else
        {
            errorResult = new ErrorSearchResult(query, error.Trim());
            return true;
        }

    }
}
