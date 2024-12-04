using GitCodeSearch.Utilities;
using System.Threading.Tasks;
using System.Threading;
using GitCodeSearch.Model;

namespace GitCodeSearch.Search.Result;

public record FileContentSearchResult(string Text, string Path, int Line, int Column, FileContentSearchQuery Query) : ISearchResult
{
    public string Expression => Query.Expression;
    public string ShortPath { get; } = System.IO.Path.GetFileName(Path);
    public string FullPath => System.IO.Path.Combine(Query.Repository.Path, Path);
    public FileType FileType { get; } = GetFileType(Path);

    public async Task<string> GetFileRemotePathAsync()
    {
        string repositoryUrl = await Query.GetRemoteUrlAsync();
        return $@"{repositoryUrl}/blob/{Query.Branch.Name}/{Path}";
    }

    public async Task<string> GetFileContentAsync(CancellationToken token)
    {
        return await GitProcess.RunAsync(Query.Repository, ["show", $"{Query.Branch ?? "HEAD"}:{Path}"], token);
    }

    public override string ToString() => $"\"{FullPath}\":{Line}:{Column} {Text}";

    private static FileType GetFileType(string path)
    {
        var extension = System.IO.Path.GetExtension(path).ToLower();
        return extension switch
        {
            ".cs" => FileType.Cs,
            ".csproj" => FileType.Csproj,
            ".xml" => FileType.Xml,
            ".json" => FileType.Json,
            ".yaml" => FileType.Yaml,
            ".js" => FileType.Js,
            ".ts" => FileType.Ts,
            ".css" => FileType.Css,
            ".htm" => FileType.Html,
            ".html" => FileType.Html,
            ".sql" => FileType.Sql,
            _ => FileType.None,
        };
    }
}
