using GitCodeSearch.Utilities;
using System.Threading.Tasks;

namespace GitCodeSearch.Model;

public record RepositoryBranch(Repository Repository, Branch Branch)
{
    public async Task<string> GetRemoteUrlAsync()
    {
        await foreach (var repositoryUrl in GitProcess.RunLinesAsync(Repository, ["remote", "get-url", Branch.Remote]))
        {
            if (repositoryUrl.EndsWith(".git"))
            {
                return repositoryUrl[..^4];
            }
            return repositoryUrl;
        }

        return string.Empty;
    }
}
