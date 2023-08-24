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
            viewModel.ShowPreviewCommand.Execute(lbi.DataContext);
        }

        private void DataGrid_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
