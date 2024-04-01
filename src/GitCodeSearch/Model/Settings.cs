using GitCodeSearch.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GitCodeSearch.Model
{
    public enum SearchType
    {
        FileContent = 0,
        CommitMessage = 1
    }

    public class Settings
    {
        public List<GitRepository> GitRepositores { get; set; } = [];

        public string? Branch { get; set; }

        [JsonIgnore]
        public string Pattern { get; set; } = "*";

        [JsonIgnore]
        public SearchType SearchType { get; set; }

        public bool IsCaseSensitive { get; set; }

        public bool IsRegex { get; set; }

        public bool ShowInactiveRepositoriesInSearchResult { get; set; }

        public string PreviewTheme { get; set; } = "vs";

        public SearchHistory SearchHistory { get; set; } = [];

        public bool UseTabs { get; set; }

        public static readonly string DefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitcodesearch");

        public static Settings Current { get; set; } = new Settings();

        public static async Task LoadAsync()
        {
            Current = await LoadAsync(DefaultPath);
        }

        public static async Task SaveAsync()
        {
            await Current.SaveAsync(DefaultPath);
        }

        private async Task SaveAsync(string path)
        {
            using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, this);
        }

        private async static Task<Settings> LoadAsync(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                var result = await JsonSerializer.DeserializeAsync<Settings>(stream);

                return result ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }
    }
}
