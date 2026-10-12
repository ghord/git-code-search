using GitCodeSearch.Model;
using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCodeSearch.Mcp;

/// <summary>
/// Maps repository and branch arguments of MCP tools to the configured model.
/// </summary>
internal static class RepositoryResolver
{
    public const string BranchDescription =
        "Branch to use, as a remote branch including the remote, e.g. 'origin/main'. " +
        "Omit it to use HEAD, the branch currently checked out in each repository - start with that. " +
        "If another branch might be wanted, ask the user which one (get_current_branch returns the branch selected in Git Code Search).";

    public static List<Repository> Resolve(string? repository)
    {
        var repositories = Settings.Current.Repositories;

        if (string.IsNullOrWhiteSpace(repository))
        {
            var active = repositories.Where(r => r.IsActiveRepository()).ToList();
            if (active.Count == 0)
                throw new McpException("No active repositories configured in Git Code Search.");
            return active;
        }

        // Only configured repositories can be accessed, never arbitrary paths.
        var match = repositories.Where(r =>
            string.Equals(r.Name, repository, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r.Path.TrimEnd('\\', '/'), repository.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase)).ToList();

        if (match.Count == 0)
            throw new McpException($"Unknown repository '{repository}'. Use list_repositories to see available repositories.");
        if (match.Count > 1)
            throw new McpException($"Repository name '{repository}' is ambiguous; use the full path.");
        if (!match[0].IsActiveRepository())
            throw new McpException($"Repository '{repository}' is inactive or not a valid git repository.");

        return match;
    }

    public static Branch ParseBranch(string? branch)
    {
        if (string.IsNullOrWhiteSpace(branch) || branch == "(local)")
            return Branch.Empty;
        if (branch.StartsWith('-'))
            throw new McpException("Invalid branch name.");
        return new Branch(branch);
    }
}
