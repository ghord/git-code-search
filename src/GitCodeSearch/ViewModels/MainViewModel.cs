using GitCodeSearch.Model;
using GitCodeSearch.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class MainViewModel : ViewModelBase
    {
        private string search_ = string.Empty;
        private readonly Window owner_;
        private Settings settings_;
        private string? currentRepository_;
        private string? branch_;

        public MainViewModel(Window owner, Settings settings)
        {
            settings_ = settings;
            SearchCommand = new RelayCommand(SearchAsync);
            SettingsCommand = new RelayCommand(SettingsAsync);
            FetchAllCommand = new RelayCommand(FetchAllAsync);
            ShowPreviewCommand = new RelayCommand<SearchResult>(ShowPreviewAsync);
            branch_ = settings.LastBranch;
            this.owner_ = owner;
        }

        private async Task ShowPreviewAsync(SearchResult searchResult)
        {
            var content = await GitHelper.GetFileContentAsync(searchResult.Query.RepositoryPath, searchResult.Path, searchResult.Query.Branch);

            if (content != null)
            {
                var viewModel = new PreviewViewModel(searchResult, content);
                var view = new PreviewView { DataContext = viewModel };
                view.Loaded += (o, e) =>
                    {

                        view.ScrollToSearchResult(searchResult);
                    };
                DialogHelper.ShowDialog(view, searchResult.Path, owner_);
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

        public string? Branch
        {
            get { return branch_; }
            set { SetField(ref branch_, value); }
        }

        public ICommand SearchCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand FetchAllCommand { get; }
        public ICommand ShowPreviewCommand { get; }

        public ObservableCollection<SearchResult> Results { get; } = new ObservableCollection<SearchResult>();

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

            HashSet<string>? branches = null;

            foreach (var repository in settings_.GetValidatedGitRepositories())
            {
                if (branches is null)
                    branches = new HashSet<string>(await GitHelper.GetBranchesAsync(repository));
                else
                    branches.IntersectWith(await GitHelper.GetBranchesAsync(repository));
            }

            Branches.Clear();

            if (branches is null)
                return;

            Branches.Add(null);

            foreach (var branch in branches.OrderBy(b => b))
            {
                Branches.Add(branch);
            }

            if (previousBranch != null && Branches.Contains(previousBranch))
                Branch = previousBranch;
            else
                Branch = null;
        }

        private async Task SearchAsync(CancellationToken token)
        {
            Results.Clear();
            CurrentRepository = null;

            foreach (var repository in settings_.GetValidatedGitRepositories())
            {
                CurrentRepository = repository;

                await foreach (var result in GitHelper.SearchAsync(Search, repository, Branch, token))
                {
                    Results.Add(result);
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
                CurrentRepository = repository;

                if (!GitHelper.IsRepository(repository))
                    continue;

                await GitHelper.FetchAsync(repository);

                if (token.IsCancellationRequested)
                    break;
            }

            CurrentRepository = null;
        }
    }
}
