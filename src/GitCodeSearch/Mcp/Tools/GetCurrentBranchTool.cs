using GitCodeSearch.Model;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace GitCodeSearch.Mcp.Tools;

[McpServerToolType]
public class GetCurrentBranchTool
{
    [McpServerTool(Name = "get_current_branch", ReadOnly = true)]
    [Description("Returns the branch currently selected in the Git Code Search UI (e.g. 'origin/main'), which the user chose to search. " +
                 "Searches default to HEAD (the checked-out branch); ask the user whether to pass this branch instead. '(local)' means the checked-out working tree of each repository.")]
    public string GetCurrentBranch() => Settings.Current.Branch.ToString();
}
