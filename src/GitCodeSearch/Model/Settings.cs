using GitCodeSearch.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GitCodeSearch.Model
{
    public class Settings
    {
        public List<GitRepository> GitRepositores { get; set; } = new();

        public string? Branch { get; set; }

        public bool IsCaseSensitive { get; set; }

        public bool IsRegex { get; set; }

        public bool ShowInactiveRepositoriesInSearchResult { get; set; }

        public string PreviewTheme { get; set; } = "vs";

        public SearchHistory SearchHistory { get; set; } = new();

        public bool UseTabs { get; set; }

        public static readonly string DefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitcodesearch");

        public static Settings Current { get; set; } = new Settings();

        public static async Task LoadAsync()
        {
            Current = await LoadAsync(DefaultPath) ?? new Settings();
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

        private async static Task<Settings?> LoadAsync(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                var result = await JsonSerializer.DeserializeAsync<Settings>(stream);

                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
