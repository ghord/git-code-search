using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GitCodeSearch.Search
{
    public abstract class Search<T> : ISearch where T : SearchQuery
    {
        protected T Query { get; }

        protected Search(SearchQuery query)
        {
            if (query is T queryT)
            {
                Query = queryT;
            }
            else
                throw new ArgumentException("Invalid query type", nameof(query));
        }

        public async IAsyncEnumerable<ISearchResult> SearchAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var psi = new ProcessStartInfo("git")
            {
                WorkingDirectory = Query.Repository.Path,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            AddArguments(psi.ArgumentList);

            using var process = Process.Start(psi);

            if (process == null)
                yield break;

            var reader = process.StandardOutput;

            while (await reader.ReadLineAsync(cancellationToken) is string line)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    process.Kill();
                    yield break;
                }

                if (TryParseSearchResult(line, out var searchResult))
                    yield return searchResult;
            }
        }

        protected abstract void AddArguments(IList<string> arguments);
        protected abstract bool TryParseSearchResult(string text, [NotNullWhen(true)] out ISearchResult? searchResult);
    }

}
