using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GitCodeSearch.Search
{
    public class FileContentSearch : Search<FileContentSearchQuery>
    {
        public FileContentSearch(SearchQuery query)
            : base(query)
        {
        }

        protected override void AddArguments(IList<string> arguments)
        {
            arguments.Add("grep");
            arguments.Add(Query.IsRegex ? "--perl-regexp" : "--fixed-strings");
            arguments.Add("--no-color");
            arguments.Add("--line-number");
            arguments.Add("--column");

            if (!Query.IsCaseSensitive)
            {
                arguments.Add("--ignore-case");
            }

            arguments.Add(Query.Expression);

            if (Query.Branch != null)
            {
                arguments.Add(Query.Branch);
            }

            arguments.Add("--");
            arguments.Add(Query.Pattern);
        }

        protected override bool TryParseSearchResult(string text, [NotNullWhen(true)] out ISearchResult? searchResult)
        {
            const int MaxLineLength = 8_000;
            searchResult = null;

            var parts = text.Split(':', 5);

            if (parts.Length < 4)
                return false;

            int offset = Query.Branch != null ? 1 : 0;

            if (Query.Branch != null && parts[0] != Query.Branch)
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
}
