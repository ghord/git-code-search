using GitCodeSearch.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Model;

public class Repository(string path, bool active)
{
    public bool Active { get; set; } = active;

    public string Path { get; set; } = path;

    [JsonIgnore]
    public string Name { get; } = System.IO.Path.GetFileName(path);

    public bool IsActiveRepository()
    {
        if (!Active)
            return false;
        if (string.IsNullOrEmpty(Path))
            return false;
        if (!Directory.Exists(Path))
            return false;
        if (!Directory.Exists(System.IO.Path.Combine(Path, ".git")))
            return false;
        return true;
    }

    public async Task FetchAsync(CancellationToken token)
    {
        await GitProcess.RunVoidAsync(this, ["fetch", "--all"], token);
    }

    public async IAsyncEnumerable<string> GetBranchesAsync()
    {
        await foreach (string line in GitProcess.RunLinesAsync(this, ["branch", "-r"]))
        {
            yield return line.Trim();
        }
    }
}
