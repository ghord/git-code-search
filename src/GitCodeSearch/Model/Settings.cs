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
        public string[]? GitRepositores { get; set; }

        public string? LastBranch { get; set; }

        public static string DefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitcodesearch");

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

        public string[] GetValidatedGitRepositories()
        {
            if (GitRepositores == null)
                return Array.Empty<string>();

            return GitRepositores.Where(GitHelper.IsRepository).ToArray();
        }

        public bool IsCaseSensitive { get; set; }

        public bool IsRegex { get; set; }
    }
}
