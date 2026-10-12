using GitCodeSearch.Search;
using GitCodeSearch.Model;
using GitCodeSearch.Search.Result;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Mcp.Tools;

[McpServerToolType]
public class SearchFileContentTool
{
    [McpServerTool(Name = "search_file_content", ReadOnly = true)]
    [Description("Searches file contents (git grep) across the user's configured local repositories and branches, without checking them out. Use this when a file, class or symbol is not found in the current project/repository, or to find usages in other repositories. By default it searches HEAD (the branch checked out in each repository); ask the user if another branch should be searched. Returns lines as 'repository: path:line:column text'.")]
    public async Task<string> SearchFileContent(
        [Description("Text or regular expression (PCRE) to search for.")] string expression,
        [Description(RepositoryResolver.BranchDescription)] string? branch = null,
        [Description("Repository name or path. Omit to search all active repositories.")] string? repository = null,
        [Description("Semicolon separated git pathspecs limiting the files searched, e.g. '*.cs;*.csproj'.")] string pattern = "*",
        [Description("Case sensitive search.")] bool caseSensitive = false,
        [Description("Treat expression as a regular expression.")] bool isRegex = false,
        [Description("Maximum number of results to return.")] int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        var resolvedBranch = RepositoryResolver.ParseBranch(branch);

        return await SearchRunner.RunAsync(repository, maxResults,
            new SearchEngine(expression)
            {
                SearchType = SearchType.FileContent,
                Pattern = pattern,
                Branch = resolvedBranch,
                IsCaseSensitive = caseSensitive,
                IsRegex = isRegex
            },
            result => result is FileContentSearchResult r
                ? $"{r.Query.Repository.Name}: {r.Path}:{r.Line}:{r.Column} {r.Text}"
                : null,
            cancellationToken);
    }
}
