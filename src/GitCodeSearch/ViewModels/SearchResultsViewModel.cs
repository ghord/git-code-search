using GitCodeSearch.Model;
using GitCodeSearch.Search;
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

namespace GitCodeSearch.ViewModels
{
    public class SearchResultsViewModel : ViewModelBase
    {
        private static Window? owner_;
        private static readonly PreviewView previewView_ = new();

        public SearchResultsViewModel(string search, Action<string?> onRepositorySearch)
        {
            OnRepositorySearch = onRepositorySearch;
            Initialize(search);

            ShowPreviewCommand = new RelayCommand<FileContentSearchResult>(ShowPreviewAsync);
            OpenFileCommand = new RelayCommand<FileContentSearchResult>(OpenFile);
            ViewFileInRemoteCommand = new RelayCommand<FileContentSearchResult>(ViewFileInRemote);
            CopyPathCommand = new RelayCommand<FileContentSearchResult>(CopyPath);
            RevealInExplorerCommand = new RelayCommand<FileContentSearchResult>(RevealInExplorer);
            CopyHashCommand = new RelayCommand<CommitMessageSearchResult>(CopyHash);
            OpenSolutionCommand = new RelayCommand<FileContentSearchResult>(OpenSolution);
            SearchRepositoryCommand = new RelayCommand<InactiveRepositorySearchResult>(SearchRepositoryAsync);
            ShowCommitCommand = new RelayCommand<CommitMessageSearchResult>(ShowCommitAsync);
            tabVisibility_ = Settings.Current.UseTabs ? Visibility.Visible : Visibility.Collapsed;
        }

        [MemberNotNull(nameof(SearchEngine))]
        public void Initialize(string search)
        {
            search_ = search;
            Results.Clear();
            SearchEngine = new SearchEngine(Search, Results, OnRepositorySearch);
        }

        public static void SetOwner(Window owner)
        {
            owner_ = owner;
        }

        public SearchEngine SearchEngine { get; private set; }

        private string search_ = string.Empty;
        public string Search
        {
            get => search_;
            set => SetField(ref search_, value);
        }

        private ObservableCollection<ISearchResult> results_ = [];
        public ObservableCollection<ISearchResult> Results
        {
            get => results_;
            set => SetField(ref results_, value);
        }

        private Visibility tabVisibility_;
        public Visibility TabVisibility
        {
            get => tabVisibility_;
            set => SetField(ref tabVisibility_, value);
        }

        public ICommand ShowPreviewCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand ViewFileInRemoteCommand { get; }
        public ICommand CopyPathCommand { get; }
        public ICommand RevealInExplorerCommand { get; }
        public ICommand CopyHashCommand { get; }
        public ICommand OpenSolutionCommand { get; }
        public ICommand SearchRepositoryCommand { get; }
        public ICommand ShowCommitCommand { get; }

        public Action<string?> OnRepositorySearch { get; internal set; }

        private async Task ShowPreviewAsync(FileContentSearchResult searchResult)
        {
            var content = await GitHelper.GetFileContentAsync(searchResult.Query.Repository.Path, searchResult.Path, searchResult.Query.Branch);

            if (content != null && owner_ != null)
            {
                previewView_.DataContext = new PreviewViewModel(searchResult, content);
                DialogHelper.ShowDialog(previewView_, searchResult.Path, owner_);
            }
        }

        private void OpenFile(FileContentSearchResult searchResult)
        {
            try
            {
                Process.Start(searchResult.FullPath);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private async Task ViewFileInRemote(FileContentSearchResult searchResult)
        {
            try
            {
                string url = await GitHelper.GetFileRemotePath(searchResult.Query.Repository.Path, searchResult.Query.Branch, searchResult.Path);

                using var _ = Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
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
                var files = directory.GetFiles("*.sln");
                if (files.Length != 0)
                {
                    ExplorerHelper.OpenFileInDefaultProgram(files[0].FullName);
                    return;
                }
                directory = directory.Parent;
            }

            MessageBox.Show($"There is no solution file in parent directories");
        }

        private async Task SearchRepositoryAsync(InactiveRepositorySearchResult searchResult, CancellationToken token)
        {
            GitRepository repository = searchResult.Query.Repository;
            int index = Results.IndexOf(searchResult); 
            await SearchEngine.SearchRepositoryAsync(repository, index, token);
        }

        private async void ShowCommitAsync(CommitMessageSearchResult searchResult)
        {
            string url = await GitHelper.GetCommitRemotePath(searchResult.Query.Repository.Path, searchResult.Query.Branch, searchResult.LongHash);

            try
            {
                using var _ = Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = url });
            }
            catch
            {

            }
        }
    }
}
