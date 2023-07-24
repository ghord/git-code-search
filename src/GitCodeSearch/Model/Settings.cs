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

        public List<string> SearchHistory { get; set; } = new();

        public static readonly string DefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitcodesearch");

        public async Task SaveAsync(string path)
        {
            using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, this);
        }

        public async static Task<Settings?> LoadAsync(string path)
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

        public GitRepository[] GetValidatedGitRepositories()
        {
            if (GitRepositores == null)
                return Array.Empty<GitRepository>();

            return GitRepositores.ToArray();
        }

        public void UpdateSearchHistory(string search)
        {
            const int MaxHistoryCount = 50;

            SearchHistory.Remove(search);
            SearchHistory.Insert(0, search);

            if (SearchHistory.Count > MaxHistoryCount)
            {
                SearchHistory.RemoveRange(MaxHistoryCount, SearchHistory.Count - MaxHistoryCount);
            }
        }
    }
}
