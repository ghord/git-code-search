using GitCodeSearch.Search;
using GitCodeSearch.Model;
using GitCodeSearch.Search.Result;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Mcp.Tools;

[McpServerToolType]
public class SearchCommitMessagesTool
{
    [McpServerTool(Name = "search_commit_messages", ReadOnly = true)]
    [Description("Searches commit history (git log --grep) across the user's configured local repositories. Use this to find when or why something changed, including in repositories other than the current project. By default it searches the history of HEAD (the checked-out branch); ask the user if another branch should be searched. Returns 'repository: hash date author message'.")]
    public async Task<string> SearchCommitMessages(
        [Description("Text or regular expression (PCRE) to search for.")] string expression,
        [Description(RepositoryResolver.BranchDescription)] string? branch = null,
        [Description("Repository name or path. Omit to search all active repositories.")] string? repository = null,
        [Description("Case sensitive search.")] bool caseSensitive = false,
        [Description("Treat expression as a regular expression.")] bool isRegex = false,
        [Description("Maximum number of results to return.")] int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        var resolvedBranch = RepositoryResolver.ParseBranch(branch);

        return await SearchRunner.RunAsync(repository, maxResults,
            new SearchEngine(expression)
            {
                SearchType = SearchType.CommitMessage,
                Branch = resolvedBranch,
                IsCaseSensitive = caseSensitive,
                IsRegex = isRegex
            },
            result => result is CommitMessageSearchResult r
                ? $"{r.Query.Repository.Name}: {r.LongHash} {r.DateTime:yyyy-MM-dd} {r.Author} {r.Message}"
                : null,
            cancellationToken);
    }
}
