using GitCodeSearch.Model;
using GitCodeSearch.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GitCodeSearch.ViewModels
{
    public enum SearchType
    {
        FileContent = 0,
        CommitMessage = 1
    }

    public class MainViewModel : ViewModelBase
    {
        private string search_ = string.Empty;
        private readonly Window owner_;
        private Settings settings_;
        private string? currentRepository_;
        private string? branch_;
        private bool isCaseSensitive_ = false;
        private bool isRegex_ = false;
        private SearchType searchType_;
        private string pattern_;
        private static readonly PreviewView previewView_ = new();

        public MainViewModel(Window owner, Settings settings)
        {
            settings_ = settings;
            SearchCommand = new RelayCommand(SearchAsync);
            SettingsCommand = new RelayCommand(SettingsAsync);
            FetchAllCommand = new RelayCommand(FetchAllAsync);
            ShowPreviewCommand = new RelayCommand<FileContentSearchResult>(ShowPreviewAsync);
            OpenFileCommand = new RelayCommand<FileContentSearchResult>(OpenFile);
            RevealInExplorerCommand = new RelayCommand<FileContentSearchResult>(RevealInExplorer);
            CopyPathCommand = new RelayCommand<FileContentSearchResult>(CopyPath);
            CopyHashCommand = new RelayCommand<CommitMessageSearchResult>(CopyHash);
            OpenSolutionCommand = new RelayCommand<FileContentSearchResult>(OpenSolution);
            SaveResultsCommand = new RelayCommand(SaveResults);
            branch_ = settings.LastBranch;
            isCaseSensitive_ = settings.IsCaseSensitive;
            isRegex_ = settings.IsRegex;
            pattern_ = "*";
            this.owner_ = owner;
        }

        private void SaveResults()
        {
            var dialog = new SaveFileDialog();

            dialog.Filter = "Text file|*.txt";
            try
            {

                if (dialog.ShowDialog(Application.Current.MainWindow) == true)
                {
                    File.WriteAllLines(dialog.FileName, Results.Select(r => r.GetText()));
                }
            }

            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public async Task SaveSettingsAsync()
        {
            await settings_.SaveAsync(Settings.DefaultPath);
        }

        private void CopyHash(CommitMessageSearchResult searchResult)
        {
            Clipboard.SetText(searchResult.Hash);

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

        private void OpenSolution(FileContentSearchResult searchResult)
        {
            var directory = Directory.GetParent(searchResult.FullPath);
            while (directory != null)
            {
                var files = directory.GetFiles("*.sln");
                if (files.Any())
                {
                    ExplorerHelper.OpenFileInDefaultProgram(files[0].FullName);
                    return;
                }
                directory = directory.Parent;
            }

            MessageBox.Show($"There is no solution file in parent directories");
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

        private async Task ShowPreviewAsync(FileContentSearchResult searchResult)
        {
            var content = await GitHelper.GetFileContentAsync(searchResult.Query.RepositoryPath, searchResult.Path, searchResult.Query.Branch);

            if (content != null)
            {
                previewView_.DataContext = new PreviewViewModel(searchResult, content);
                DialogHelper.ShowDialog(previewView_, searchResult.Path, owner_);
            }
        }

        public ObservableCollection<string?> Branches { get; } = new ObservableCollection<string?> { null };


        public string Search
        {
            get { return search_; }
            set { SetField(ref search_, value); }
        }

        public string? CurrentRepository
        {
            get { return currentRepository_; }
            set { SetField(ref currentRepository_, value); }
        }


        public bool IsCaseSensitive
        {
            get { return isCaseSensitive_; }
            set
            {
                settings_.IsCaseSensitive = value;
                SetField(ref isCaseSensitive_, value);
            }
        }


        public bool IsRegex
        {
            get { return isRegex_; }
            set
            {
                settings_.IsRegex = value;
                SetField(ref isRegex_, value);
            }
        }

        public string? Branch
        {
            get { return branch_; }
            set { SetField(ref branch_, value); }
        }


        public SearchType SearchType
        {
            get { return searchType_; }
            set { SetField(ref searchType_, value); }
        }

        public string Pattern
        {
            get { return pattern_; }
            set { SetField(ref pattern_, value); }
        }


        public ICommand SearchCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand FetchAllCommand { get; }
        public ICommand ShowPreviewCommand { get; }
        public ICommand RevealInExplorerCommand { get; }
        public ICommand CopyPathCommand { get; }
        public ICommand SaveResultsCommand { get; }
        public ICommand CopyHashCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand OpenSolutionCommand { get; }
        public ObservableCollection<ISearchResult> Results { get; } = new ObservableCollection<ISearchResult>();

        private async Task SettingsAsync()
        {
            var viewModel = new SettingsViewModel(settings_);
            var view = new SettingsView { DataContext = viewModel };

            if (DialogHelper.ShowDialog(view, "Settings", owner_))
            {
                settings_ = viewModel.GetSettings();
                await settings_.SaveAsync(Settings.DefaultPath);


                await UpdateBranchesAsync();

            }
        }

        public async Task UpdateBranchesAsync()
        {
            var previousBranch = Branch;

            Dictionary<string, int> branchStats = new Dictionary<string, int>();

            foreach (var repository in settings_.GetValidatedGitRepositories())
            {
                var branches = await GitHelper.GetBranchesAsync(repository);

                foreach(var branch in branches)
                {
                    if(branchStats.TryGetValue(branch, out int count))
                    {
                        branchStats[branch] = ++count;
                    }
                    else
                    {
                        branchStats[branch] = 1;
                    }
                }
            }

            Branches.Clear();


            Branches.Add(null);

            foreach (var branch in branchStats.OrderBy(b => b.Key.ToLower()))
            {
                Branches.Add(branch.Key);
            }

            if (previousBranch != null && Branches.Contains(previousBranch))
                Branch = previousBranch;
            else
                Branch = null;
            RaisePropertyChanged(nameof(Branches));
        }

        private async Task SearchAsync(CancellationToken token)
        {
            Results.Clear();
            CurrentRepository = null;

            if (string.IsNullOrEmpty(Search))
                return;

            foreach (var repository in settings_.GetValidatedGitRepositories())
            {
                CurrentRepository = repository.Path;

                switch (searchType_)
                {
                    case SearchType.FileContent:
                        {
                            var query = new FileContentSearchQuery(Search, Pattern, Branch, repository.Path, IsCaseSensitive, IsRegex);

                            await foreach (var result in GitHelper.SearchFileContentAsync(query, token))
                            {
                                if (token.IsCancellationRequested)
                                    break;
                                Results.Add(result);
                            }
                            break;
                        }
                    case SearchType.CommitMessage:
                        {
                            var query = new CommitMessageSearchQuery(Search, Branch, repository.Path, IsCaseSensitive, IsRegex);

                            await foreach (var result in GitHelper.SearchCommitMessageAsync(query, token))
                            {
                                if (token.IsCancellationRequested)
                                    break;
                                Results.Add(result);
                            }
                            break;
                        }
                }

                if (token.IsCancellationRequested)
                    break;

            }

            if (settings_.LastBranch != branch_)
            {
                settings_.LastBranch = branch_;
                await settings_.SaveAsync(Settings.DefaultPath);
            }

            CurrentRepository = null;
        }

        private async Task FetchAllAsync(CancellationToken token)
        {
            CurrentRepository = null;

            if (settings_.GitRepositores == null)
                return;

            foreach (var repository in settings_.GitRepositores)
            {
                CurrentRepository = repository.Path;

                if (!GitHelper.IsRepository(repository))
                    continue;

                await GitHelper.FetchAsync(repository, token);

                if (token.IsCancellationRequested)
                    break;
            }

            CurrentRepository = null;
        }
    }
}
