using GitCodeSearch.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
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

    public static readonly string DefaultPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "gitcodesearch.json");
    private static readonly string OldDefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitcodesearch");

    public static Settings Current { get; set; } = new Settings();

    public static void Load()
    {
        try
        {
            Current = ReadSettingsFromOldPath() ?? ReadSettingsFromFile(DefaultPath) ?? new();
        }
        catch (Exception ex)
        {
            Current = new();
        }
    }

    public static void Save()
    {
        using var stream = File.Create(DefaultPath);
        JsonSerializer.Serialize(stream, Current);
    }

    private static Settings? ReadSettingsFromOldPath()
    {
        if (File.Exists(OldDefaultPath))
        {
            var settings = ReadSettingsFromFile(OldDefaultPath);
            if (settings != null)
            {
                File.Delete(OldDefaultPath);
                return settings;
            }
        }

        return null;
    }

    private static Settings? ReadSettingsFromFile(string path)
    {
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<Settings>(stream);
    }
}
