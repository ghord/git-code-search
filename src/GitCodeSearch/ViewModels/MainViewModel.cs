using GitCodeSearch.Model;
using GitCodeSearch.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
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
        private string? searchedRepository_;
        private CancellationTokenSource cts_ = new CancellationTokenSource();

        public MainViewModel(Window owner, Settings settings)
        {
            settings_ = settings;

            SearchCommand = new RelayCommand(SearchAsync);
            SettingsCommand = new RelayCommand(SettingsAsync);
            CancelSearchCommand = new RelayCommand(CancelSearch);
            this.owner_ = owner;
        }

        private void CancelSearch()
        {
            cts_.Cancel();
        }

        public string Search
        {
            get { return search_; }
            set { SetField(ref search_, value); }
        }

        public string? SearchedRepository
        {
            get { return searchedRepository_; }
            set { SetField(ref searchedRepository_, value); }
        }

        public ICommand SearchCommand { get; }
        public ICommand SettingsCommand { get; }

        public ICommand CancelSearchCommand { get; }

        public ObservableCollection<SearchResult> Results { get; } = new ObservableCollection<SearchResult>();

        private async Task SettingsAsync()
        {
            var viewModel = new SettingsViewModel(settings_);
            var view = new SettingsView { DataContext = viewModel };

            if (DialogHelper.ShowDialog(view, "Settings", owner_))
            {
                settings_ = viewModel.GetSettings();
                await settings_.SaveAsync(Settings.DefaultPath);
            }
        }

        private async Task SearchAsync()
        {
            Results.Clear();
            SearchedRepository = null;

            cts_.Dispose();
            cts_ = new CancellationTokenSource();

            if (settings_.GitRepositores == null)
                return;

            foreach (var repository in settings_.GitRepositores)
            {
                SearchedRepository = repository;

                if (!Directory.Exists(repository) || !Directory.Exists(Path.Combine(repository, ".git")))
                    continue;

                await foreach (var result in GitHelper.SearchAsync(Search, repository, cts_.Token))
                {
                    Results.Add(result);
                }

                if (cts_.IsCancellationRequested)
                    break;
            }

            SearchedRepository = null;
        }
    }
}
