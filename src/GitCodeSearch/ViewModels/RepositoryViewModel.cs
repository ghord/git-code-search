using GitCodeSearch.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GitCodeSearch.ViewModels
{
    public class GitRepositoryViewModel : ViewModelBase
    {
        public GitRepositoryViewModel() 
        {
            path_ = string.Empty;
        }

        public GitRepositoryViewModel(GitRepository gitRepository)
        {
            active_ = gitRepository.Active;
            path_= gitRepository.Path;
        }

        private bool active_;
        public bool Active
        {
            get => active_;
            set
            {
                if (active_ != value)
                {
                    active_ = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string path_;
        public string Path
        {
            get => path_;
            set
            {
                if (path_ != value)
                {
                    path_ = value;
                    RaisePropertyChanged();
                }
            }
        }
    }

    public class GitRepositoriesViewModel : ObservableCollection<GitRepositoryViewModel>
    {
        public GitRepositoriesViewModel(List<GitRepository> gitRepositories)
        {
            foreach(var repository in gitRepositories)
            {
                Add(new GitRepositoryViewModel(repository));
            }
        }

        public List<GitRepository> ToList()
        {
            var list = new List<GitRepository>();

            foreach(var repositoryViewModel in this)
            {
                list.Add(new GitRepository 
                { 
                    Active = repositoryViewModel.Active, 
                    Path = repositoryViewModel.Path 
                });
            }

            return list;
        }
    }

}
