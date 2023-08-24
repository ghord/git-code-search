using GitCodeSearch.Model;
using GitCodeSearch.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace GitCodeSearch.ViewModels
{
    public enum SettingsSection
    {
        [Description("Git")]
        Git,

        [Description("User interface")]
        UserInterface,
    }

    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            Initialize(Settings.Current);
        }

        public GitRepositoriesViewModel GitRepositories { get; set; }
        public string? Branch { get; set; }
        public bool IsCaseSensitive { get; set; }
        public bool IsRegex { get; set; }
        public bool ShowInactiveRepositoriesInSearchResult { get; set; }
        public string PreviewTheme { get; set; }
        public SearchHistory SearchHistory { get; set; }
        public bool UseTabs { get; set; }

        public static IEnumerable<SettingsSection> SettingsSections => Enum.GetValues<SettingsSection>();

        public static IEnumerable<string> PreviewThemes
        {
            get
            {
                yield return "vs";
                yield return "vs-dark";
                yield return "hc-black";
                yield return "monokai";
                yield return "github-dark";
                yield return "github-light";
                yield return "clouds";
                yield return "clouds-midnight";
                yield return "xcode-default";
                yield return "pastels-on-dark";
            }
        }

        [MemberNotNull(nameof(GitRepositories), nameof(SearchHistory), nameof(PreviewTheme))]
        private void Initialize(Settings settings)
        {
            GitRepositories = new GitRepositoriesViewModel(settings.GitRepositores);
            Branch = settings.Branch;
            IsCaseSensitive = settings.IsCaseSensitive;
            IsRegex = settings.IsRegex;
            ShowInactiveRepositoriesInSearchResult = settings.ShowInactiveRepositoriesInSearchResult;
            PreviewTheme = settings.PreviewTheme;
            SearchHistory = settings.SearchHistory;
            UseTabs = settings.UseTabs;
        }

        internal async Task ApplySettings()
        {
            Settings.Current = new Settings
            {
                GitRepositores = GitRepositories.ToList(),
                Branch = Branch,
                IsCaseSensitive = IsCaseSensitive,
                IsRegex = IsRegex,
                ShowInactiveRepositoriesInSearchResult = ShowInactiveRepositoriesInSearchResult,
                PreviewTheme = PreviewTheme,
                SearchHistory = SearchHistory,
                UseTabs = UseTabs,
            };
            await Settings.SaveAsync();
        }
    }
}
