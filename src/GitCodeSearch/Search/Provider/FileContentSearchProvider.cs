using GitCodeSearch.Model;
using GitCodeSearch.Search.Result;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GitCodeSearch.Search.Provider;

public class FileContentSearchProvider(FileContentSearchQuery Query) : AbstractSearchProvider(Query)
{
    public override Repository Repository => Query.Repository;

    public override IEnumerable<string> GetArguments()
    {
        yield return "grep";
        yield return Query.IsRegex ? "--perl-regexp" : "--fixed-strings";
        yield return "--no-color";
        yield return "--line-number";
        yield return "--column";

        if (!Query.IsCaseSensitive)
        {
            yield return "--ignore-case";
        }

        yield return Query.Expression;

        if (Query.Branch != Branch.Empty)
        {
            yield return Query.Branch;
        }

        yield return "--";

        foreach (var pattern in Query.Pattern.Split(';'))
        {
            yield return pattern;
        }
    }

    public override bool TryParseSearchResult(string text, [NotNullWhen(true)] out ISearchResult? searchResult)
    {
        const int MaxLineLength = 8_000;
        searchResult = null;

        var parts = text.Split(':', 5);

        if (parts.Length < 4)
            return false;

        int offset = Query.Branch != Branch.Empty ? 1 : 0;

        if (Query.Branch != Branch.Empty && parts[0] != Query.Branch)
            return false;

        var path = parts[offset];

        if (!int.TryParse(parts[offset + 1], out int line))
            return false;

        if (!int.TryParse(parts[offset + 2], out int column))
            return false;

        string trimmedLine = parts[offset + 3].Substring(0, Math.Min(parts[offset + 3].Length, MaxLineLength)).TrimStart();
        searchResult = new FileContentSearchResult(trimmedLine, path, line, column, Query);

        return true;
    }
}

