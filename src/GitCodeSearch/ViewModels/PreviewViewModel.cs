using GitCodeSearch.Search;

namespace GitCodeSearch.ViewModels
{
    public class PreviewViewModel(FileContentSearchResult searchResult, string content) : ViewModelBase
    {
        public FileContentSearchResult SearchResult { get; } = searchResult;
        public string Content { get; } = content;
    }
}
