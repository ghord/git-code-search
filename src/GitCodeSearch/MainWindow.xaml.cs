using GitCodeSearch.Model;
using GitCodeSearch.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitCodeSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadDataContextAsync().WaitAndDispatch();
        }

        private async Task LoadDataContextAsync()
        {
            await Settings.LoadAsync();
            var viewModel = new MainViewModel(this);
            await viewModel.UpdateBranchesAsync();
            this.DataContext = viewModel;
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            await Settings.SaveAsync();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;
            CancelSearch();

            viewModel.SearchCommand.Execute(null);
        }

        private void CancelSearch()
        {
            var viewModel = (MainViewModel)DataContext;
            if (viewModel.SearchCommand.IsRunning)
            {
                viewModel.SearchCommand.CancelCommand?.Execute(null);
            }
        }

        private void CloseButton_Click(object sender, MouseButtonEventArgs e)
        {
            CloseTabItem(sender);
        }

        private void TabItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                CloseTabItem(sender);
            }
        }

        private void CloseTabItem(object sender)
        {
            if (sender is FrameworkElement frameworkElement)
            {
                var viewModel = (MainViewModel)DataContext;
                var searchResultViewModel = (SearchResultsViewModel)frameworkElement.DataContext;

                if (viewModel.IsActiveSearchResults(searchResultViewModel))
                {
                    CancelSearch();
                }

                viewModel.SearchResults.Remove(searchResultViewModel);
            }
        }

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e?.Source is TabItem tabItem)
            {
                if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
                {
                    DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
                }
            }
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Source is TabItem targetTabItem &&
                e.Data.GetData(typeof(TabItem)) is TabItem sourceTabItem &&
                !targetTabItem.Equals(sourceTabItem))
            {
                var viewModel = (MainViewModel)DataContext;
                var sourceSearchResults = (SearchResultsViewModel)sourceTabItem.DataContext;
                var targetSearchResults = (SearchResultsViewModel)targetTabItem.DataContext;
                int targetIndex = viewModel.SearchResults.IndexOf(targetSearchResults);

                viewModel.SearchResults.Remove(sourceSearchResults);
                viewModel.SearchResults.Insert(targetIndex, sourceSearchResults);
                viewModel.ActiveTabIndex = targetIndex;
            }
        }
    }
}
