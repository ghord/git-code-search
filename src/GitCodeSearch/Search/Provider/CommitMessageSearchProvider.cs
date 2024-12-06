using GitCodeSearch.Model;
using GitCodeSearch.Search.Result;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GitCodeSearch.Search.Provider;

public class CommitMessageSearchProvider(CommitMessageSearchQuery Query) : AbstractSearchProvider(Query)
{
    public override Repository Repository => Query.Repository;

    public override IEnumerable<string> GetArguments()
    {
        yield return "log";
        yield return Query.IsRegex ? "--perl-regexp" : "--fixed-strings";
        yield return "--grep=" + Query.Expression;
        yield return "--pretty=format:%h%x09%H%x09%an%x09%ad%x09%s";
        yield return "--date=iso";

        if (!Query.IsCaseSensitive)
        {
            yield return "--regexp-ignore-case";
        }

        if (Query.Branch != Branch.Empty)
        {
            yield return Query.Branch;
        }
    }

    public override bool TryParseSearchResult(string text, [NotNullWhen(true)] out ISearchResult? searchResult)
    {
        searchResult = null;

        var parts = text.Split('\t', 5);

        if (parts.Length < 5)
            return false;

        var hash = parts[0];
        var longHash = parts[1];
        var author = parts[2];
        var message = parts[4];

        if (!DateTime.TryParse(parts[3], null, DateTimeStyles.RoundtripKind, out var date))
            return false;

        searchResult = new CommitMessageSearchResult(message, hash, longHash, author, date, Query);

        return true;
    }
}
