using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace GitCodeSearch.Model
{
    public class SearchQuery
    {
        public string? Branch { get; }
        public string Expression { get; }
        public string Repository { get; }
        public string RepositoryPath { get; }
        public string Pattern { get; }

        public bool IsCaseSensitive { get; }
        public SearchQuery(string expression, string repositoryPath, string? branch, string pattern, bool isCaseSensitive)
        {
            Expression = expression;
            Repository = Path.GetFileName(Path.GetDirectoryName(repositoryPath)) ?? string.Empty;
            Branch = branch;
            RepositoryPath = repositoryPath;
            Pattern = pattern;
            IsCaseSensitive = isCaseSensitive;
        }
    }

    public class SearchResult
    {
        public SearchResult(SearchQuery query,  string path, int line, int column, string text)
        {
            Text = text;
            Path = path;
            Line = line;
            Query = query;
            Column = column;
        }

        public string Text { get; }
        public string Path { get; }
        public int Line { get; }
        public int Column { get; }
        public SearchQuery Query { get; }

        public string FullPath => System.IO.Path.Combine(Query.RepositoryPath, Path);
    }


    public static class GitHelper
    {
        public static async IAsyncEnumerable<SearchResult> SearchAsync(SearchQuery searchQuery, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var psi = new ProcessStartInfo("git");
            psi.WorkingDirectory = searchQuery.RepositoryPath;
            psi.ArgumentList.Add("grep");
            psi.ArgumentList.Add("-F");
            psi.ArgumentList.Add("--no-color");
            psi.ArgumentList.Add("--line-number");
            psi.ArgumentList.Add("--column");

            if (!searchQuery.IsCaseSensitive)
                psi.ArgumentList.Add("--ignore-case");

            psi.ArgumentList.Add(searchQuery.Expression);

            if (searchQuery.Branch != null)
                psi.ArgumentList.Add(searchQuery.Branch);

            psi.ArgumentList.Add("--");
            psi.ArgumentList.Add(searchQuery.Pattern);

            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;

            var process = Process.Start(psi);

            if (process == null)
                yield break;

            var reader = process.StandardOutput;


            while (await reader.ReadLineAsync() is string line)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    process.Kill();
                    yield break;
                }

                if (TryParseSearchResult(line, searchQuery, out var searchResult))
                    yield return searchResult;
            }
        }

       

        private static bool TryParseSearchResult(string text, SearchQuery query, [NotNullWhen(true)] out SearchResult? searchResult)
        {
            searchResult = null;

            var idx = text.IndexOf(':');
            int lastIdx = 0;

            if (idx < 0)
                return false;

            if (query.Branch != null)
            {
                if (text.Substring(0, idx) != query.Branch)
                    return false;

                idx = text.IndexOf(':', lastIdx = idx +1);

                if (idx < 0)
                    return false;
            }

            var path = text[lastIdx..idx];

            idx = text.IndexOf(':', lastIdx = idx + 1);

            if (idx < 0)
                return false;

            if (!int.TryParse(text[lastIdx..idx], out int line))
                return false;

            idx = text.IndexOf(':', lastIdx = idx + 1);

            if (idx < 0)
                return false;

            if (!int.TryParse(text[lastIdx..idx], out int column))
                return false;

            searchResult = new SearchResult(query, path, line, column, text[(idx+1)..]);

            return true;
        }

        public static async Task<string?> GetFileContentAsync(string repository, string path, string? branch)
        {
            var psi = new ProcessStartInfo("git");

            psi.WorkingDirectory = repository;
            psi.ArgumentList.Add("show");
            psi.ArgumentList.Add($"{branch ?? "HEAD"}:{path}");

            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.RedirectStandardOutput = true;

            psi.CreateNoWindow = true;

            var process = Process.Start(psi);

            if (process == null)
                return null;

            var content = await process.StandardOutput.ReadToEndAsync();

            await process.WaitForExitAsync();

            return content;
        }

        public static async Task FetchAsync(string repository)
        {
            var psi = new ProcessStartInfo("git");

            psi.WorkingDirectory = repository;
            psi.ArgumentList.Add("fetch");
            psi.ArgumentList.Add("--all");
            psi.CreateNoWindow = true;

            var process = Process.Start(psi);

            if (process == null)
                return;

            await process.WaitForExitAsync();
        }

        public static bool IsRepository(string repository)
        {
            if (string.IsNullOrEmpty(repository))
                return false;
            if (!Directory.Exists(repository))
                return false;
            if (!Directory.Exists(Path.Combine(repository, ".git")))
                return false;
            return true;
        }

        public static async Task<string[]> GetBranchesAsync(string repository)
        {
            var psi = new ProcessStartInfo("git");

            psi.WorkingDirectory = repository;
            psi.ArgumentList.Add("branch");
            psi.ArgumentList.Add("-r");
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;

            var process = Process.Start(psi);

            if (process == null)
                return Array.Empty<string>();

            await process.WaitForExitAsync();

            var reader = process.StandardOutput;

            var result = new List<string>();

            while (reader.ReadLine() is string line)
            {
                result.Add(line.Trim());
            }

            return result.ToArray();
        }
    }
}
