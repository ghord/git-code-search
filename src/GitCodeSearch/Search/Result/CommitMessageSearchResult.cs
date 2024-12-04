using System.Threading.Tasks;
using System;

namespace GitCodeSearch.Search.Result;

public record CommitMessageSearchResult(string Message, string Hash, string LongHash, string Author, DateTime DateTime,
        CommitMessageSearchQuery Query) : ISearchResult
{
    public async Task<string> GetCommitRemotePathAsync()
    {
        string repositoryUrl = await Query.GetRemoteUrlAsync();

        if (repositoryUrl.Contains("/gitlab/"))
        {
            return $@"{repositoryUrl}/-/commit/{Hash}";
        }
        else
        {
            return $@"{repositoryUrl}/commit/{Hash}";
        }
    }

    override public string ToString() => Hash + " " + Message;
}
