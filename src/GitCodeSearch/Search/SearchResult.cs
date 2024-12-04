using System;

namespace GitCodeSearch.Search
{
    public interface ISearchResult
    {
        SearchQuery Query { get; }
        string GetText();
    }

    public abstract record SearchResult<TSearchQuery>(TSearchQuery Query) : ISearchResult where TSearchQuery : SearchQuery
    {
        SearchQuery ISearchResult.Query => Query;

        public abstract string GetText();
    }

    public record FileContentSearchResult(string Text, string Path, int Line, int Column, FileContentSearchQuery Query) : SearchResult<FileContentSearchQuery>(Query)
    {
        public string ShortPath { get; } = System.IO.Path.GetFileName(Path);
        public string FullPath => System.IO.Path.Combine(Query.Repository.Path, Path);

        public override string GetText()
        {
            return $"\"{FullPath}\":{Line}:{Column} {Text}";
        }
    }

    public record CommitMessageSearchResult(string Message, string Hash, string LongHash, string Author, DateTime DateTime, CommitMessageSearchQuery Query) : SearchResult<CommitMessageSearchQuery>(Query)
    {
        public override string GetText()
        {
            return Hash + " " + Message;
        }
    }

    public record InactiveRepositorySearchResult(SearchQuery Query, int Count) : SearchResult<SearchQuery>(Query)
    {
        public string Text => GetText();

        public override string GetText()
        {
            return $"{Count} result{(Count == 1 ? "" : "s")} (inactive)";
        }
    }

    public record MissingBranchRepositorySearchResult(SearchQuery Query) : SearchResult<SearchQuery>(Query)
    {
        public string Text => GetText();

        public override string GetText()
        {
            
            return $"Missing branch {Query.Branch} {(Query.Repository.Active ? "" : "(inactive)")}";
        }
    }


}
