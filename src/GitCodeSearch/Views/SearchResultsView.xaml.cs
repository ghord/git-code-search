using GitCodeSearch.Search;
using GitCodeSearch.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitCodeSearch.Views
{
    /// <summary>
    /// Interaction logic for SearchResultsView.xaml
    /// </summary>
    public partial class SearchResultsView : UserControl
    {
        public SearchResultsView()
        {
            InitializeComponent();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lbi = (ListBoxItem)sender;
            var viewModel = (SearchResultsViewModel)DataContext;
            switch (lbi.DataContext)
            {
                case FileContentSearchResult fileContentSearchResult:
                    viewModel.ShowPreviewCommand.Execute(fileContentSearchResult);
                    break;

                case CommitMessageSearchResult commitMessageSearchResult:
                    viewModel.ShowCommitCommand.Execute(commitMessageSearchResult);
                    break;

                case InactiveRepositorySearchResult inactiveRepositorySearchResult:
                    viewModel.SearchRepositoryCommand.Execute(inactiveRepositorySearchResult);
                    break;
            }
        }

        private void DataGrid_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
