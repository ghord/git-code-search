using GitCodeSearch.Model;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Linq;

namespace GitCodeSearch.Mcp.Tools;

[McpServerToolType]
public class ListRepositoriesTool
{
    [McpServerTool(Name = "list_repositories", ReadOnly = true)]
    [Description("Lists the git repositories configured in Git Code Search (name, path, active/inactive). Use it to get valid repository names for the other tools.")]
    public string ListRepositories()
    {
        var repositories = Settings.Current.Repositories;
        if (repositories.Count == 0)
            return "No repositories configured. Add them in the Git Code Search settings.";

        return string.Join('\n', repositories.Select(r =>
            $"{r.Name}\t{r.Path}\t{(r.IsActiveRepository() ? "active" : "inactive")}"));
    }
}
