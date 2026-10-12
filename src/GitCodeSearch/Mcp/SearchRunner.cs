using GitCodeSearch.Model;
using GitCodeSearch.Search;
using GitCodeSearch.Search.Result;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Mcp;

/// <summary>
/// Runs a search engine over the requested repositories and formats the results as text.
/// </summary>
internal static class SearchRunner
{
    public static async Task<string> RunAsync(
        string? repository,
        int maxResults,
        SearchEngine engine,
        Func<ISearchResult, string?> format,
        CancellationToken cancellationToken)
    {
        maxResults = Math.Clamp(maxResults, 1, 1000);
        var output = new List<string>();
        bool truncated = false;

        foreach (var repo in RepositoryResolver.Resolve(repository))
        {
            await foreach (var result in engine.GetResultsAsync(repo, cancellationToken))
            {
                if (output.Count >= maxResults)
                {
                    truncated = true;
                    break;
                }

                string? text = result is ErrorSearchResult or MissingBranchRepositorySearchResult
                    ? $"{repo.Name}: {result}" // errors (e.g. missing branch) are reported as text, like the UI shows them
                    : format(result);

                if (text != null)
                    output.Add(text);
            }

            if (truncated)
                break;
        }

        if (output.Count == 0)
            return "No results.";

        return string.Join('\n', output) + (truncated ? $"\n... more results available; stopped at {maxResults}." : string.Empty);
    }
}
