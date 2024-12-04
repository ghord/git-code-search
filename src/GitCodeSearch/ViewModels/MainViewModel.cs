using GitCodeSearch.Model;
using GitCodeSearch.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GitCodeSearch.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        SearchController = new SearchController(UpdateCurrentRepository);

        SearchCommand = new SelfCancelledCommand(SearchAsync, () => !string.IsNullOrEmpty(SearchController.SearchText));
        SettingsCommand = new RelayCommand(SettingsAsync);
        FetchAllCommand = new RelayCommand(FetchAllAsync);
        SaveResultsCommand = new RelayCommand(SaveResultsAsync, () => SearchController.Any());
        CloseTabCommand = new RelayCommand<SearchViewModel>(CloseTab);
        CloseOtherTabsCommand = new RelayCommand<SearchViewModel>(CloseOtherTabs, (s) => SearchController.Count > 1);
        CloseAllTabsCommand = new RelayCommand(CloseAllTabs);

        _ = UpdateBranchesAsync();
    }

    public bool IsCaseSensitive
    {
        get => Settings.Current.IsCaseSensitive;
        set => SetField(ref Settings.Current.IsCaseSensitive, value);
    }

    public bool IsRegex
    {
        get => Settings.Current.IsRegex;
        set => SetField(ref Settings.Current.IsRegex, value);
    }

    public Branch Branch
    {
        get => Settings.Current.Branch;
        set => SetField(ref Settings.Current.Branch, value);
    }

    public SearchType SearchType
    {
        get => Settings.Current.SearchType;
        set => SetField(ref Settings.Current.SearchType, value);
    }

    public string Pattern
    {
        get => Settings.Current.Pattern;
        set => SetField(ref Settings.Current.Pattern, value);
    }

    public Repository? CurrentRepository
    {
        get => field;
        set => SetField(ref field, value);
    }

    public ObservableCollection<string> SearchHistory
    {
        get => field;
        set => SetField(ref field, value);
    } = new(Settings.Current.SearchHistory);

    public SearchController SearchController { get; }
    public ObservableCollection<Branch> Branches { get; } = [Branch.Empty];


    public async Task UpdateBranchesAsync()
    {
        var currentBranch = Branch;
        var branches = await Settings.Current.Repositories.GetAllBranchesAsync();

        Branches.Clear();
        Branches.Add(Branch.Empty);

        foreach (var branch in Settings.Current.FavouriteBranches)
        {
            branch.IsFavourite = true;
            Branches.Add(branch);
            branches.Remove(branch);
        }

        foreach (var branch in branches.OrderBy(branch => branch.ToLower()))
        {
            Branches.Add(new Branch(branch));
        }

        Branch = Branches.FirstOrDefault(b => string.Equals(b, currentBranch)) ?? Branch.Empty;
    }

    private void UpdateCurrentRepository(Repository? repository)
    {
        CurrentRepository = repository;
    }

    #region Commands declaration and methods

    public RelayCommand SearchCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand FetchAllCommand { get; }
    public ICommand SaveResultsCommand { get; }
    public ICommand CloseTabCommand { get; }
    public ICommand CloseOtherTabsCommand { get; }
    public ICommand CloseAllTabsCommand { get; }

    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        var searchText = SearchController.SearchText;
        if (!string.IsNullOrEmpty(searchText))
        {
            UpdateSearchHistory(searchText);
            await SearchController.SearchAsync(cancellationToken);
        }
    }

    private async Task SettingsAsync()
    {
        var viewModel = new SettingsViewModel();
        var view = new SettingsView { DataContext = viewModel };

        if (DialogHelper.ShowDialog(view, "Settings"))
        {
            viewModel.ApplySettings();
            await UpdateBranchesAsync();
            SearchController.UpdateTabsVisibility();
        }
    }

    private async Task FetchAllAsync(CancellationToken cancellationToken)
    {
        await Settings.Current.Repositories.FetchAllAsync(UpdateCurrentRepository, cancellationToken);
    }

    private async Task SaveResultsAsync()
    {
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "Text file|*.txt" };
            if (dialog.ShowDialog(Application.Current.MainWindow) == true)
            {
                await File.WriteAllLinesAsync(dialog.FileName, SearchController.Current.Results.Select(r => r.ToString() ?? string.Empty));
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }

    private void UpdateSearchHistory(string search)
    {
        Settings.Current.SearchHistory.Add(search);
        SearchHistory = new ObservableCollection<string>(Settings.Current.SearchHistory);
    }

    internal void CloseTab(SearchViewModel searchResultViewModel)
    {
        if (SearchController.Current == searchResultViewModel)
        {
            SearchCommand.Cancel();
        }
        SearchController.Remove(searchResultViewModel);
    }

    private void CloseOtherTabs(SearchViewModel searchViewModel)
    {
        SearchController.RemoveAllExcept(searchViewModel);
    }

    private void CloseAllTabs()
    {
        SearchController.Clear();
    }
    #endregion
}
