using GitCodeSearch.Search;
using System.Windows;
using System.Windows.Controls;

namespace GitCodeSearch.Views
{
    public class SearchResultTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? FileContentTemplate { get; set; }

        public DataTemplate? CommitMessageTemplate { get; set; }

        public DataTemplate? InactiveRepositoryTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                FileContentSearchResult => FileContentTemplate,
                CommitMessageSearchResult => CommitMessageTemplate,
                InactiveRepositorySearchResult => InactiveRepositoryTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
