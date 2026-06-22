using GitCodeSearch.Utilities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace GitCodeSearch.Model;

public enum SearchType
{
    FileContent = 0,
    CommitMessage = 1
}

public class Settings
{
    [JsonPropertyName("GitRepositores")]
    public Repositories Repositories { get; set; } = [];

    [JsonInclude]
    public Branch Branch = Branch.Empty;

    [JsonIgnore]
    public string Pattern = "*";

    [JsonIgnore]
    public SearchType SearchType;

    /// <summary>
    /// Gets or sets the regular expression for invalid branch names.
    /// </summary>
    [StringSyntax("Regex")]
    public string InvalidBranchRegex { get; set; } = @"(.*\/cherry-pick-\w*)|(.*\/revert-\w*)|(.*\/.*-patch-\d*)|(.*\/TFS-\d+.*)";

    [JsonInclude]
    public bool IsCaseSensitive;

    [JsonInclude]
    public bool IsRegex;

    public bool ShowInactiveRepositoriesInSearchResult { get; set; }

    public bool WarnOnMissingBranch { get; set; } = true;

    public string PreviewTheme { get; set; } = "vs";

    public SearchHistory SearchHistory { get; set; } = [];

    public bool UseTabs { get; set; }

    public List<Branch> FavouriteBranches { get; set; } = [];

    public static Settings Current { get; set; } = new Settings();
}
