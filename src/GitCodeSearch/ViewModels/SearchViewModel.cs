using GitCodeSearch.Model;
using GitCodeSearch.Search;
using GitCodeSearch.Search.Result;
using GitCodeSearch.Utilities;
using GitCodeSearch.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GitCodeSearch.ViewModels;

public class SearchViewModel : ViewModelBase
{
    private static readonly PreviewView previewView_ = new();

    public SearchViewModel(string searchText, Action<Repository?> onRepositorySearch)
    {
        OnRepositorySearch = onRepositorySearch;
        Initialize(searchText);

        ShowPreviewCommand = new RelayCommand<FileContentSearchResult>(ShowPreviewAsync);
        OpenFileCommand = new RelayCommand<FileContentSearchResult>(OpenFile);
        ViewFileInRemoteCommand = new RelayCommand<FileContentSearchResult>(ViewFileInRemoteAsync);
        CopyPathCommand = new RelayCommand<FileContentSearchResult>(CopyPath);
        RevealInExplorerCommand = new RelayCommand<FileContentSearchResult>(RevealInExplorer);
        CopyHashCommand = new RelayCommand<CommitMessageSearchResult>(CopyHash);
        OpenSolutionCommand = new RelayCommand<FileContentSearchResult>(OpenSolution);
        SearchRepositoryCommand = new RelayCommand<InactiveRepositorySearchResult>(SearchRepositoryAsync);
        ShowCommitCommand = new RelayCommand<CommitMessageSearchResult>(ShowCommitAsync);
        ActivateRepositoryCommand = new RelayCommand<InactiveRepositorySearchResult>(ActivateRepositoryAsync);
    }

    [MemberNotNull(nameof(SearchEngine))]
    public void Initialize(string searchText)
    {
        Search = searchText;
        Results.Clear();
        SearchEngine = new SearchEngine(Search, Results, OnRepositorySearch);
    }

    private SearchEngine SearchEngine { get; set; }

    public string Search
    {
        get => field;
        set => SetField(ref field, value);
    } = string.Empty;

    public ObservableCollection<ISearchResult> Results
    {
        get => field;
        set => SetField(ref field, value);
    } = [];

    public Visibility TabVisibility
    {
        get => field;
        set => SetField(ref field, value);
    } = Settings.Current.UseTabs ? Visibility.Visible : Visibility.Collapsed;

    public ICommand ShowPreviewCommand { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand ViewFileInRemoteCommand { get; }
    public ICommand CopyPathCommand { get; }
    public ICommand RevealInExplorerCommand { get; }
    public ICommand CopyHashCommand { get; }
    public ICommand OpenSolutionCommand { get; }
    public ICommand SearchRepositoryCommand { get; }
    public ICommand ShowCommitCommand { get; }
    public ICommand ActivateRepositoryCommand { get; }

    public Action<Repository?> OnRepositorySearch { get; internal set; }

    public async Task SearchAsync(CancellationToken cancellationToken) => await SearchEngine.SearchAsync(cancellationToken);

    private async Task ShowPreviewAsync(FileContentSearchResult searchResult, CancellationToken cancellationToken)
    {
        string content = await searchResult.GetFileContentAsync(cancellationToken);
        previewView_.DataContext = new PreviewViewModel(searchResult, content);
        DialogHelper.ShowWindow(previewView_, searchResult.Path);
    }

    private void OpenFile(FileContentSearchResult searchResult)
    {
        ExplorerHelper.OpenFileInDefaultProgram(searchResult.FullPath);
    }

    private async Task ViewFileInRemoteAsync(FileContentSearchResult searchResult)
    {
        string url = await searchResult.GetFileRemotePathAsync();
        ExplorerHelper.OpenUrlInDefaultBrowser(url);
    }

    private void CopyPath(FileContentSearchResult searchResult)
    {
        try
        {
            Clipboard.SetText(new Uri(searchResult.FullPath).LocalPath);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private void RevealInExplorer(FileContentSearchResult searchResult)
    {
        var filePath = searchResult.FullPath;

        if (!File.Exists(filePath))
        {
            MessageBox.Show($"File {filePath} does not exist");
            return;
        }

        if (Path.GetDirectoryName(filePath) is string directory && Path.GetFileName(filePath) is string fileName)
        {
            ExplorerHelper.OpenFolderAndSelectItem(directory, fileName);
        }
    }

    private void CopyHash(CommitMessageSearchResult searchResult)
    {
        Clipboard.SetText(searchResult.Hash);
    }

    private void OpenSolution(FileContentSearchResult searchResult)
    {
        var directory = Directory.GetParent(searchResult.FullPath);
        while (directory != null)
        {
            if (directory.Exists)
            {
                var files = directory.GetFiles("*.sln");
                if (files.Length != 0)
                {
                    ExplorerHelper.OpenFileInDefaultProgram(files[0].FullName);
                    return;
                }
            }
            directory = directory.Parent;
        }

        MessageBox.Show($"There is no solution file in parent directories");
    }

    private async void ShowCommitAsync(CommitMessageSearchResult searchResult)
    {
        string url = await searchResult.GetCommitRemotePathAsync();
        ExplorerHelper.OpenUrlInDefaultBrowser(url);
    }

    private async Task SearchRepositoryAsync(InactiveRepositorySearchResult searchResult, CancellationToken cancellationToken)
    {
        int index = Results.IndexOf(searchResult);
        await SearchEngine.SearchRepositoryAsync(searchResult.Repository, index, cancellationToken);
    }

    private async Task ActivateRepositoryAsync(InactiveRepositorySearchResult searchResult, CancellationToken cancellationToken)
    {
        searchResult.Repository.Active = true;
        await SearchRepositoryAsync(searchResult, cancellationToken);
    }
}
