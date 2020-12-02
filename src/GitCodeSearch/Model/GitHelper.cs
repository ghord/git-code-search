using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Model
{
    public class SearchResult
    {
        public SearchResult(string repository, string text, string path)
        {
            Text = text;
            Path = path;
            Repository = repository;
        }

        public string Text { get; }
        public string Path { get; }
        public string Repository { get; }

    }


    public static class GitHelper
    {
        public static async IAsyncEnumerable<SearchResult> SearchAsync(string query, string repository, string? branch = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var psi = new ProcessStartInfo("git");
            psi.WorkingDirectory = repository;
            psi.ArgumentList.Add("grep");
            psi.ArgumentList.Add("-F");
            psi.ArgumentList.Add("--no-color");
            psi.ArgumentList.Add("--line-number");
            psi.ArgumentList.Add("--column");
            psi.ArgumentList.Add(query);

            if (branch != null)
                psi.ArgumentList.Add(branch);

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

                var indexOfTab = line.IndexOf(':');

                if (indexOfTab >= 0)
                    yield return new SearchResult(repository, line[indexOfTab..], line[..indexOfTab]);
            }
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

            while(reader.ReadLine() is string line)
            {
                result.Add(line.Trim());
            }

            return result.ToArray();
        }
    }
}
