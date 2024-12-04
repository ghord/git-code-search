using GitCodeSearch.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GitCodeSearch.ViewModels;

public class RepositoryViewModel : ViewModelBase
{
    public RepositoryViewModel(string path, bool active = false)
    {
        Path = path;
        Active = active;
    }

    public RepositoryViewModel(Repository repository)
    {
        Active = repository.Active;
        Path = repository.Path;
    }

    public bool Active
    {
        get => field;
        set => SetField(ref field, value);
    }

    public string Path
    {
        get => field;
        set => SetField(ref field, value);
    }
}

public class RepositoriesViewModel : ObservableCollection<RepositoryViewModel>
{
    public RepositoriesViewModel(List<Repository> repositories)
    {
        foreach(var repository in repositories)
        {
            Add(new RepositoryViewModel(repository));
        }
    }

    public static implicit operator Repositories(RepositoriesViewModel viewModel)
    {
        var repositories = new Repositories();
        foreach (var repository in viewModel)
        {
            repositories.Add(new Repository(repository.Path, repository.Active));
        }
        return repositories;
    }
}
