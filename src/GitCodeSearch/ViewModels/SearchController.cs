using GitCodeSearch.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GitCodeSearch.ViewModels;

public class SearchController(Action<Repository?> OnRepositorySearch) : ObservableCollection<SearchViewModel>
{
    public string SearchText 
    { 
        get => field;
        set
        {
            field = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SearchText)));
        }
    } = string.Empty;

    public int CurrentIndex 
    { 
        get => field;
        set
        {
            field = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentIndex)));
        } 
    } = -1;

    public SearchViewModel Current => this[CurrentIndex];

    public async Task SearchAsync(CancellationToken cancellationToken)
    {
        await GetOrAddSearchViewModel().SearchAsync(cancellationToken);
    }

    public void UpdateTabsVisibility()
    {
        foreach (var searchViewModel in this)
        {
            searchViewModel.TabVisibility = Settings.Current.UseTabs ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private SearchViewModel GetOrAddSearchViewModel()
    {
        SearchViewModel searchViewModel;
        int index;

        if (Settings.Current.UseTabs)
        {
            index = FindIndex(r => r.Search == SearchText);
            if (index != -1)
            {
                searchViewModel = ReuseSearchSearchViewModel(index);
            }
            else
            {
                searchViewModel = AddSearchViewModel();
                index = Count - 1;
            }
        }
        else
        {
            index = 0;
            if (this.Any())
            {
                searchViewModel = ReuseSearchSearchViewModel(index);
            }
            else
            {
                searchViewModel = AddSearchViewModel();
            }
        }

        CurrentIndex = index;

        return searchViewModel;
    }

    internal void MoveTo(SearchViewModel source, SearchViewModel target)
    {
        int targetIndex = IndexOf(target);

        Remove(source);
        Insert(targetIndex, source);
        CurrentIndex = targetIndex;
    }

    internal void RemoveAllExcept(SearchViewModel searchResultViewModel)
    {
        Clear();
        Add(searchResultViewModel);
        CurrentIndex = 0;
    }

    private SearchViewModel AddSearchViewModel()
    {
        var searchResults = new SearchViewModel(SearchText, OnRepositorySearch);
        Add(searchResults);
        return searchResults;
    }

    private SearchViewModel ReuseSearchSearchViewModel(int index)
    {
        SearchViewModel searchResults = this[index];
        searchResults.Initialize(SearchText);
        return searchResults;
    }

    private int FindIndex(Func<SearchViewModel, bool> predicate)
    {
        for (int index = 0; index < Count; index++)
        {
            if (predicate(this[index]))
                return index;
        }

        return -1;
    }
}
