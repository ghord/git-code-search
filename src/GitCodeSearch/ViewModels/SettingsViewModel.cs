using GitCodeSearch.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCodeSearch.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(Settings settings)
        {
            gitRepositories_ = settings.GitRepositores is null ? 
                string.Empty :
                string.Join(Environment.NewLine, settings.GitRepositores);
        }

        private string gitRepositories_;

        public string GitRepositories
        {
            get { return gitRepositories_; }
            set { SetField(ref gitRepositories_, value); }
        }

        public Settings GetSettings()
        {
            return new Settings
            {
                GitRepositores = gitRepositories_.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            };
        }
    }
}
