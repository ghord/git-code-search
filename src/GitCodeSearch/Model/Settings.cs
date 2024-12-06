using GitCodeSearch.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
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

    public static readonly string DefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitcodesearch");

    public static Settings Current { get; set; } = new Settings();

    public static void Load() => Current = Load(DefaultPath);
    public static void Save() => Current.Save(DefaultPath);

    private void Save(string path)
    {
        using var stream = File.Create(path);
        JsonSerializer.Serialize(stream, this);
    }

    private static Settings Load(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize<Settings>(stream) ?? new Settings();
        }
        catch
        {
            return new Settings();
        }
    }
}
