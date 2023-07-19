using GitCodeSearch.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCodeSearch.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(Settings settings)
        {
            gitRepositories_ = new GitRepositoriesViewModel(settings.GitRepositores);
            ShowInactiveRepositoriesInSearchResult = settings.ShowInactiveRepositoriesInSearchResult;
        }

        private GitRepositoriesViewModel gitRepositories_;

        public GitRepositoriesViewModel GitRepositories
        {
            get { return gitRepositories_; }
            set { SetField(ref gitRepositories_, value); }
        }

        public bool ShowInactiveRepositoriesInSearchResult { get; set; }

        public Settings GetSettings()
        {
            return new Settings
            {
                GitRepositores = gitRepositories_.ToList(),
                ShowInactiveRepositoriesInSearchResult = ShowInactiveRepositoriesInSearchResult,
            };
        }
    }
}
