using GitCodeSearch.Model;
using GitCodeSearch.Utilities;
using GitCodeSearch.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        private string? currentRepository_;
        private SearchType searchType_;
        private string pattern_ = "*";
        private ObservableCollection<string> searchHistory_ = new(Settings.Current.SearchHistory);
        private ObservableCollection<SearchResultsViewModel> searchResults_ = new ObservableCollection<SearchResultsViewModel>();
        private int activeTabIndex_ = -1;

        public MainViewModel(Window owner)
        {
            owner_ = owner;
            SearchCommand = new RelayCommand(SearchAsync);
            SettingsCommand = new RelayCommand(SettingsAsync);
            FetchAllCommand = new RelayCommand(FetchAllAsync);
            SaveResultsCommand = new RelayCommand(SaveResults, CanExecuteSaveResults);
            CloseTabCommand = new RelayCommand<SearchResultsViewModel>(CloseTab);
            CloseOtherTabsCommand = new RelayCommand<SearchResultsViewModel>(CloseOtherTabs, CanExecuteCloseOtherTabs);
            CloseAllTabsCommand = new RelayCommand(CloseAllTabs);

            SearchResultsViewModel.Initialize(owner_);
            if (!Settings.Current.UseTabs)
            {
                AddSearchResults();
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
            get => Settings.Current.IsCaseSensitive;
            set
            {
                Settings.Current.IsCaseSensitive = value;
                RaisePropertyChanged();
            }
        }


        public bool IsRegex
        {
            get => Settings.Current.IsRegex;
            set
            {
                Settings.Current.IsRegex = value;
                RaisePropertyChanged();
            }
        }

        public string? Branch
        {
            get => Settings.Current.Branch;
            set
            {
                Settings.Current.Branch = value;
                RaisePropertyChanged();
            }
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

        public ObservableCollection<string> SearchHistory
        {
            get => searchHistory_;
            set => SetField(ref searchHistory_, value);
        }

        public int ActiveTabIndex
        {
            get => activeTabIndex_;
            set => SetField(ref activeTabIndex_, value);
        }

        public ObservableCollection<SearchResultsViewModel> SearchResults
        {
            get => searchResults_;
            private set => SetField(ref searchResults_, value);
        }

        public bool IsActiveSearchResults(SearchResultsViewModel searchResults)
        {
            return SearchResults[ActiveTabIndex] == searchResults;
        }

        public RelayCommand SearchCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand FetchAllCommand { get; }
        public ICommand SaveResultsCommand { get; }
        public ICommand CloseTabCommand { get; }
        public ICommand CloseOtherTabsCommand { get; }
        public ICommand CloseAllTabsCommand { get; }


        private async Task SettingsAsync()
        {
            var viewModel = new SettingsViewModel();
            var view = new SettingsView { DataContext = viewModel };

            if (DialogHelper.ShowDialog(view, "Settings", owner_))
            {
                await viewModel.ApplySettings();
                await UpdateBranchesAsync();
                UpdateTabsVisibility();
            }
        }

        private void UpdateTabsVisibility()
        {
            foreach (var searchResultViewModel in SearchResults)
            {
                searchResultViewModel.TabVisibility = Settings.Current.UseTabs ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public async Task UpdateBranchesAsync()
        {
            var previousBranch = Branch;

            Dictionary<string, int> branchStats = new Dictionary<string, int>();

            foreach (var repository in Settings.Current.GitRepositores)
            {
                if (!GitHelper.IsActiveRepository(repository))
                    continue;

                var branches = await GitHelper.GetBranchesAsync(repository);

                foreach (var branch in branches)
                {
                    if (branchStats.TryGetValue(branch, out int count))
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

        private SearchResultsViewModel GetOrAddSearchResults()
        {
            SearchResultsViewModel searchResults;
            int index;

            if (Settings.Current.UseTabs)
            {
                index = SearchResults.FindIndex(r => r.Search == search_);
                if (index != -1)
                {
                    searchResults = ReuseSearchResults(index);
                }
                else
                {
                    searchResults = AddSearchResults();
                    index = SearchResults.Count - 1;
                }
            }
            else
            {
                index = 0;
                if (SearchResults.Any())
                {
                    searchResults = ReuseSearchResults(index);
                }
                else
                {
                    searchResults = AddSearchResults();
                }
            }

            ActiveTabIndex = index;

            return searchResults;
        }

        private SearchResultsViewModel AddSearchResults()
        {
            var searchResults = new SearchResultsViewModel { Search = search_ };
            SearchResults.Add(searchResults);
            return searchResults;
        }

        private SearchResultsViewModel ReuseSearchResults(int index)
        {
            SearchResultsViewModel searchResults = SearchResults[index];
            searchResults.Search = search_;
            searchResults.Results.Clear();
            return searchResults;
        }

        private async Task SearchAsync(CancellationToken token)
        {
            if (string.IsNullOrEmpty(Search))
                return;

            SearchResultsViewModel searchResults = GetOrAddSearchResults();
            string search = search_;

            UpdateSearchHistory(search);

            foreach (var repository in Settings.Current.GitRepositores.OrderByDescending(GitHelper.IsActiveRepository))
            {
                CurrentRepository = repository.Path;

                if (!GitHelper.IsActiveRepository(repository) && !Settings.Current.ShowInactiveRepositoriesInSearchResult)
                {
                    continue;
                }

                switch (searchType_)
                {
                    case SearchType.FileContent:
                        {
                            var query = new FileContentSearchQuery(search, Pattern, Branch, repository.Path, IsCaseSensitive, IsRegex);

                            if (GitHelper.IsActiveRepository(repository))
                            {
                                await foreach (var result in GitHelper.SearchFileContentAsync(query, token).WithCancellation(token))
                                {
                                    searchResults.Results.Add(result);
                                }
                            }
                            else
                            {
                                var inactiveQuery = new InactiveRepositorySearchQuery(query);

                                int count = 0;
                                await foreach (var result in GitHelper.SearchFileContentAsync(query).WithCancellation(token))
                                {
                                    count++;
                                }

                                if (count > 0 && !token.IsCancellationRequested)
                                {
                                    searchResults.Results.Add(new InactiveRepositorySearchResult(inactiveQuery, count));
                                }
                            }
                            break;
                        }
                    case SearchType.CommitMessage:
                        {
                            var query = new CommitMessageSearchQuery(search, Branch, repository.Path, IsCaseSensitive, IsRegex);


                            if (GitHelper.IsActiveRepository(repository))
                            {

                                await foreach (var result in GitHelper.SearchCommitMessageAsync(query, token).WithCancellation(token))
                                {
                                    if (token.IsCancellationRequested)
                                        break;
                                    searchResults.Results.Add(result);
                                }
                            }
                            else
                            {
                                var inactiveQuery = new InactiveRepositorySearchQuery(query);

                                int count = 0;
                                await foreach (var result in GitHelper.SearchCommitMessageAsync(query).WithCancellation(token))
                                {
                                    count++;
                                    break;
                                }

                                if (count > 0 && !token.IsCancellationRequested)
                                {
                                    searchResults.Results.Add(new InactiveRepositorySearchResult(inactiveQuery, count));
                                }
                            }


                            break;
                        }
                }

                if (token.IsCancellationRequested)
                    break;

            }

            CurrentRepository = null;
        }

        private void UpdateSearchHistory(string search)
        {
            Settings.Current.SearchHistory.Add(search);
            SearchHistory = new ObservableCollection<string>(Settings.Current.SearchHistory);
        }

        private async Task FetchAllAsync(CancellationToken token)
        {
            CurrentRepository = null;

            if (Settings.Current.GitRepositores == null)
                return;

            foreach (var repository in Settings.Current.GitRepositores)
            {
                CurrentRepository = repository.Path;

                if (!GitHelper.IsActiveRepository(repository))
                    continue;

                await GitHelper.FetchAsync(repository, token);

                if (token.IsCancellationRequested)
                    break;
            }

            CurrentRepository = null;
        }

        private void SaveResults()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Text file|*.txt"
            };

            try
            {
                if (dialog.ShowDialog(Application.Current.MainWindow) == true)
                {
                    File.WriteAllLines(dialog.FileName, SearchResults[ActiveTabIndex].Results.Select(r => r.GetText()));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private bool CanExecuteSaveResults()
        {
            return SearchResults.Any();
        }

        private void CloseTab(SearchResultsViewModel searchResultViewModel)
        {
            SearchResults.Remove(searchResultViewModel);
        }

        private void CloseOtherTabs(SearchResultsViewModel searchResultViewModel)
        {
            SearchResults = new ObservableCollection<SearchResultsViewModel>(new[] { searchResultViewModel });
            ActiveTabIndex = 0;
        }

        private bool CanExecuteCloseOtherTabs(SearchResultsViewModel searchResultViewModel)
        {
            return SearchResults.Count > 1;
        }

        private void CloseAllTabs()
        {
            SearchResults.Clear();
        }
    }
}
