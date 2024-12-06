using GitCodeSearch.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitCodeSearch;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel ViewModel
    {
        get => field;
        set
        {
            field = value;
            DataContext = value;
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
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
        if (sender is FrameworkElement frameworkElement 
            && frameworkElement.DataContext is SearchViewModel searchResultsViewModel)
        {
            ViewModel.CloseTab(searchResultsViewModel);
        }
    }

    private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.Source is TabItem tabItem)
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
            var sourceSearchResults = (SearchViewModel)sourceTabItem.DataContext;
            var targetSearchResults = (SearchViewModel)targetTabItem.DataContext;

            ViewModel.SearchController.MoveTo(sourceSearchResults, targetSearchResults);
        }
    }
}
