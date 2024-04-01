using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Model
{
    public static class GitHelper
    {
        public static async Task<string?> GetFileContentAsync(string repository, string path, string? branchOrCommit)
        {
            var psi = new ProcessStartInfo("git")
            {
                WorkingDirectory = repository,
                StandardOutputEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            psi.ArgumentList.Add("show");
            psi.ArgumentList.Add($"{branchOrCommit ?? "HEAD"}:{path}");

            using var process = Process.Start(psi);

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
            var psi = new ProcessStartInfo("git")
            {
                WorkingDirectory = repository.Path,
                CreateNoWindow = true
            };

            psi.ArgumentList.Add("fetch");
            psi.ArgumentList.Add("--all");

            using var process = Process.Start(psi);

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

        public static async Task<IEnumerable<string>> GetBranchesAsync(GitRepository repository)
        {
            var psi = new ProcessStartInfo("git")
            {
                WorkingDirectory = repository.Path,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            psi.ArgumentList.Add("branch");
            psi.ArgumentList.Add("-r");

            using var process = Process.Start(psi);

            if (process == null)
                return [];

            var reader = process.StandardOutput;

            var result = new List<string>();

            while (reader.ReadLine() is string line)
            {
                result.Add(line.Trim());
            }

            await process.WaitForExitAsync();

            return result;
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

            using var process = Process.Start(psi);

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

        public static async Task<string> GetCommitRemotePath(string repositoryPath, string? branch, string commitHash)
        {
            string repositoryUrl = await GetRemoteUrl(repositoryPath, branch, true);

            if(repositoryUrl.Contains("/gitlab/"))
            {
                return $@"{repositoryUrl}/-/commit/{commitHash}";
            }
            else
            {
                return $@"{repositoryUrl}/commit/{commitHash}";
            }
        }

        public static async Task<string> GetFileRemotePath(string repositoryPath, string? branch, string filePath)
        {
            string repositoryUrl = await GetRemoteUrl(repositoryPath, branch, true);

            return $@"{repositoryUrl}/blob/{GetBranch(branch)}/{filePath}";
        }  
    }
}
