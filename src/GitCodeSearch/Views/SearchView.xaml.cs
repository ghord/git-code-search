using GitCodeSearch.Search.Result;
using GitCodeSearch.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitCodeSearch.Views
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {
        public SearchView()
        {
            InitializeComponent();
        }

        private void DataGrid_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
