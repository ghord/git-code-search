using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GitCodeSearch.Search
{
    public class CommitMessageSearch : Search<CommitMessageSearchQuery>
    {
        public CommitMessageSearch(SearchQuery query)
            : base(query)
        {
        }

        protected override void AddArguments(IList<string> arguments)
        {
            arguments.Add("log");
            arguments.Add(Query.IsRegex ? "--perl-regexp" : "--fixed-strings");
            arguments.Add("--grep=" + Query.Expression);
            arguments.Add("--pretty=format:%h%x09%H%x09%an%x09%ad%x09%s");
            arguments.Add("--date=iso");

            if (!Query.IsCaseSensitive)
            {
                arguments.Add("--regexp-ignore-case");
            }

            if (Query.Branch != null)
            {
                arguments.Add(Query.Branch);
            }
        }

        protected override bool TryParseSearchResult(string text, [NotNullWhen(true)] out ISearchResult? searchResult)
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
}
