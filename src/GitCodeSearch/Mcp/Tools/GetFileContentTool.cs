using GitCodeSearch.Model;
using GitCodeSearch.Utilities;
using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Mcp.Tools;

[McpServerToolType]
public class GetFileContentTool
{
    private const int MaxFileCharacters = 100_000;

    [McpServerTool(Name = "get_file_content", ReadOnly = true)]
    [Description("Returns the content of a file in a repository at a given branch (git show). Use after search_file_content to read a file found in another repository or branch. Reads from HEAD unless a branch is given.")]
    public async Task<string> GetFileContent(
        [Description("Repository name or path.")] string repository,
        [Description("File path relative to the repository root, as returned by search_file_content.")] string path,
        [Description(RepositoryResolver.BranchDescription)] string? branch = null,
        [Description("First line to return (1-based). Omit to start at the beginning.")] int? startLine = null,
        [Description("Last line to return (inclusive). Omit to read to the end.")] int? endLine = null,
        CancellationToken cancellationToken = default)
    {
        var repo = RepositoryResolver.Resolve(repository).Single();
        var resolvedBranch = RepositoryResolver.ParseBranch(branch);
        string revision = resolvedBranch == Branch.Empty ? "HEAD" : resolvedBranch;

        string content = await GitProcess.RunAsync(repo, ["show", $"{revision}:{path}"], cancellationToken);
        if (content.Length == 0)
            return $"File '{path}' not found or empty at {revision} in {repo.Name}.";

        if (startLine.HasValue || endLine.HasValue)
        {
            var lines = content.Split('\n');
            int start = Math.Max(1, startLine ?? 1);
            int end = Math.Min(lines.Length, endLine ?? lines.Length);
            var sb = new StringBuilder();
            for (int i = start; i <= end; i++)
                sb.Append(i).Append(": ").AppendLine(lines[i - 1].TrimEnd('\r'));
            content = sb.ToString();
        }

        if (content.Length > MaxFileCharacters)
            content = content[..MaxFileCharacters] + $"\n... truncated at {MaxFileCharacters} characters; use startLine/endLine to read more.";

        return content;
    }
}
