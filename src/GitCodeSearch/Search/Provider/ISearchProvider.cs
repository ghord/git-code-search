using GitCodeSearch.Model;
using GitCodeSearch.Search.Result;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GitCodeSearch.Search.Provider;

public interface ISearchProvider
{
    Repository Repository { get; }

    IEnumerable<string> GetArguments();
    bool TryParseSearchResult(string text, [NotNullWhen(true)] out ISearchResult? searchResult);
}
