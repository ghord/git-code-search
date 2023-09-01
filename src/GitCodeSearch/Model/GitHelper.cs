using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    public abstract record SearchQuery(string? Branch, string RepositoryPath, bool IsCaseSensitive, bool IsRegex)
    {
        public string Repository { get; } = Path.GetFileName(RepositoryPath) ?? string.Empty;
    }

    public record FileContentSearchQuery(string Expression, string Pattern, string? Branch, string RepositoryPath, bool IsCaseSensitive, bool IsRegex)
        : SearchQuery(Branch, RepositoryPath, IsCaseSensitive, IsRegex);

    public record CommitMessageSearchQuery(string Expression, string? Branch, string RepositoryPath, bool IsCaseSensitive, bool IsRegex)
        : SearchQuery(Branch, RepositoryPath, IsCaseSensitive, IsRegex);

    public record InactiveRepositorySearchQuery(SearchQuery originalQuery) :
        SearchQuery(originalQuery.Branch, originalQuery.Repository, originalQuery.IsCaseSensitive, originalQuery.IsRegex);

    public interface ISearchResult
    {
        SearchQuery Query { get; }
        string GetText();
    }

    public abstract record SearchResult<TSearchQuery>(TSearchQuery Query) : ISearchResult where TSearchQuery : SearchQuery
    {
        SearchQuery ISearchResult.Query => Query;

        public abstract string GetText();
    }

    public record FileContentSearchResult(string Text, string Path, int Line, int Column, FileContentSearchQuery Query) : SearchResult<FileContentSearchQuery>(Query)
    {
        public string ShortPath { get; } = System.IO.Path.GetFileName(Path);
        public string FullPath => System.IO.Path.Combine(Query.RepositoryPath, Path);

        public override string GetText()
        {
            return $"\"{FullPath}\":{Line}:{Column} {Text}";
        }
    }

    public record CommitMessageSearchResult(string Message, string Hash, string Author, DateTime DateTime, CommitMessageSearchQuery Query) : SearchResult<CommitMessageSearchQuery>(Query)
    {
        public override string GetText()
        {
            return Hash + " " + Message;
        }
    }

    public record InactiveRepositorySearchResult(InactiveRepositorySearchQuery Query, int Count) : SearchResult<InactiveRepositorySearchQuery>(Query)
    {
        public string Text => GetText();

        public override string GetText()
        {
            return $"{Count} result{(Count == 1 ? "" : "s")} (inactive)";
        }
    }

    public static class GitHelper
    {
        public static async IAsyncEnumerable<CommitMessageSearchResult> SearchCommitMessageAsync(CommitMessageSearchQuery searchQuery, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var psi = new ProcessStartInfo("git");
            psi.WorkingDirectory = searchQuery.RepositoryPath;
            psi.ArgumentList.Add("log");
            psi.ArgumentList.Add(searchQuery.GetRegexArgument());
            psi.ArgumentList.Add("--grep=" + searchQuery.Expression);
            psi.ArgumentList.Add("--pretty=format:%h%x09%an%x09%ad%x09%s");
            psi.ArgumentList.Add("--date=iso");
            if (!searchQuery.IsCaseSensitive)
                psi.ArgumentList.Add("--regexp-ignore-case");

            if (searchQuery.Branch != null)
                psi.ArgumentList.Add(searchQuery.Branch);

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

                if (TryParseCommitMessageSearchResult(line, searchQuery, out var searchResult))
                    yield return searchResult;
            }
        }

        public static async IAsyncEnumerable<FileContentSearchResult> SearchFileContentAsync(FileContentSearchQuery searchQuery, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var psi = new ProcessStartInfo("git");
            psi.WorkingDirectory = searchQuery.RepositoryPath;
            psi.ArgumentList.Add("grep");
            psi.ArgumentList.Add(searchQuery.GetRegexArgument());
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

                if (TryParseFileContentSearchResult(line, searchQuery, out var searchResult))
                    yield return searchResult;
            }
        }


        private static bool TryParseCommitMessageSearchResult(string text, CommitMessageSearchQuery query, [NotNullWhen(true)] out CommitMessageSearchResult? searchResult)
        {
            searchResult = null;


            var idx = text.IndexOf('\t');
            int lastIdx = 0;

            if (idx < 0)
                return false;

            var hash = text[lastIdx..idx];

            idx = text.IndexOf('\t', lastIdx = idx + 1);

            if (idx < 0)
                return false;

            var author = text[lastIdx..idx];

            idx = text.IndexOf('\t', lastIdx = idx + 1);

            if (idx < 0)
                return false;

            if (!DateTime.TryParse(text[lastIdx..idx], null, DateTimeStyles.RoundtripKind, out var date))
                return false;

            searchResult = new CommitMessageSearchResult(text[(idx + 1)..], hash, author, date, query);

            return true;
        }


        private static bool TryParseFileContentSearchResult(string text, FileContentSearchQuery query, [NotNullWhen(true)] out FileContentSearchResult? searchResult)
        {
            const int MaxLineLength = 8_000;
            searchResult = null;

            var idx = text.IndexOf(':');
            int lastIdx = 0;

            if (idx < 0)
                return false;

            if (query.Branch != null)
            {
                if (text.Substring(0, idx) != query.Branch)
                    return false;

                idx = text.IndexOf(':', lastIdx = idx + 1);

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

            string trimmedLine = text[(idx + 1)..Math.Min(text.Length, MaxLineLength)].TrimStart();
            searchResult = new FileContentSearchResult(trimmedLine, path, line, column, query);

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

        public static string GetRemote(string? branch)
        {
            return branch?.Split("/").First() ?? "origin";
        }
        
        public static string GetBranch(string? branch)
        {
            return branch?.Split("/").Last() ?? "main";
        }

        public static async Task FetchAsync(GitRepository repository, CancellationToken token)
        {
            var psi = new ProcessStartInfo("git");

            psi.WorkingDirectory = repository.Path;
            psi.ArgumentList.Add("fetch");
            psi.ArgumentList.Add("--all");
            psi.CreateNoWindow = true;

            var process = Process.Start(psi);

            if (process == null)
                return;

            await process.WaitForExitAsync(token);
        }

        public static bool IsActiveRepository(GitRepository repository)
        {
            if (!repository.Active)
                return false;
            if (string.IsNullOrEmpty(repository.Path))
                return false;
            if (!Directory.Exists(repository.Path))
                return false;
            if (!Directory.Exists(Path.Combine(repository.Path, ".git")))
                return false;
            return true;
        }

        public static async Task<string[]> GetBranchesAsync(GitRepository repository)
        {
            var psi = new ProcessStartInfo("git");

            psi.WorkingDirectory = repository.Path;
            psi.ArgumentList.Add("branch");
            psi.ArgumentList.Add("-r");
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;

            var process = Process.Start(psi);

            if (process == null)
                return Array.Empty<string>();

            var reader = process.StandardOutput;

            var result = new List<string>();

            while (reader.ReadLine() is string line)
            {
                result.Add(line.Trim());
            }

            await process.WaitForExitAsync();

            return result.ToArray();
        }

        public static async Task<string> GetRemoteUrl(string repositoryPath, string? branch, bool stripGitExtension)
        {
            var psi = new ProcessStartInfo("git")
            {
                WorkingDirectory = repositoryPath,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            psi.ArgumentList.Add("remote");
            psi.ArgumentList.Add("get-url");
            psi.ArgumentList.Add(GetRemote(branch));

            var process = Process.Start(psi);

            if (process == null)
                return string.Empty;

            var reader = process.StandardOutput;

            string repositoryUrl = string.Empty;

            if (reader.ReadLine() is string line)
            {
                repositoryUrl = line;
            }

            await process.WaitForExitAsync();

            if(stripGitExtension && repositoryUrl.EndsWith(".git"))
            {
                repositoryUrl = repositoryUrl[..^4];
            }

            return repositoryUrl;
        }


        public static async Task<string> GetFileRemotePath(string repositoryPath, string? branch, string filePath)
        {
            string repositoryUrl = await GetRemoteUrl(repositoryPath, branch, true);

            return $@"{repositoryUrl}/blob/{GetBranch(branch)}/{filePath}";
        }

        private static string GetRegexArgument(this SearchQuery searchQuery)
        {
            if (searchQuery.IsRegex)
                return "--perl-regexp";
            else
                return "--fixed-strings";
        }
    }
}
