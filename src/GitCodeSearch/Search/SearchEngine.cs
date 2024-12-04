using GitCodeSearch.Model;
using GitCodeSearch.Search.Provider;
using GitCodeSearch.Search.Result;
using GitCodeSearch.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Search
{
    public class SearchEngine(string Search, ObservableCollection<ISearchResult> Results, Action<Repository?> OnRepositorySearch)
    {
        public SearchType SearchType { get; } = Settings.Current.SearchType;
        public string Pattern { get; } = Settings.Current.Pattern;
        public Branch Branch { get; } = Settings.Current.Branch;
        public bool IsCaseSensitive { get; } = Settings.Current.IsCaseSensitive;
        public bool IsRegex { get; } = Settings.Current.IsRegex;

        public async Task SearchAsync(CancellationToken cancellationToken)
        {
            foreach (var repository in Settings.Current.Repositories.OrderByDescending(r => r.IsActiveRepository()))
            {
                OnRepositorySearch.Invoke(repository);

                if (!repository.IsActiveRepository() && !Settings.Current.ShowInactiveRepositoriesInSearchResult)
                {
                    continue;
                }

                ISearchProvider searchProvider = CreateSearchProvider(repository);

                if (repository.IsActiveRepository())
                {
                    await foreach (var result in SearchRepositoryInternalAsync(searchProvider, cancellationToken))
                    {
                        Results.Add(result);
                    }
                }
                else
                {
                    int count = 0;
                    await foreach (var result in SearchRepositoryInternalAsync(searchProvider, cancellationToken))
                    {
                        if(result is not MissingBranchRepositorySearchResult)
                            count++;
                        else
                        {
                            Results.Add(result);
                            break;
                        }
                    }

                    if (count > 0 && !cancellationToken.IsCancellationRequested)
                    {
                        Results.Add(new InactiveRepositorySearchResult(repository, count));
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            OnRepositorySearch.Invoke(null);
        }

        public async Task SearchRepositoryAsync(Repository repository, int index, CancellationToken cancellationToken)
        {
            bool first = true;
            ISearchProvider searchProvider = CreateSearchProvider(repository);
            await foreach (var result in SearchRepositoryInternalAsync(searchProvider, cancellationToken))
            {
                if (first)
                {
                    first = false;
                    Results[index] = result;
                }
                else
                {
                    Results.Insert(index, result);
                }

                index++;
            }
        }

        private static async IAsyncEnumerable<ISearchResult> SearchRepositoryInternalAsync(ISearchProvider provider, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var line in GitProcess.RunLinesAsync(provider.Repository, provider.GetArguments(), cancellationToken))
            {
                if (provider.TryParseSearchResult(line, out var searchResult))
                    yield return searchResult;
            }
        }

        private ISearchProvider CreateSearchProvider(Repository repository)
        {
            return SearchType switch
            {
                SearchType.FileContent => new FileContentSearchProvider(CreateSearchQuery<FileContentSearchQuery>(repository)),
                SearchType.CommitMessage => new CommitMessageSearchProvider(CreateSearchQuery<CommitMessageSearchQuery>(repository)),
                _ => throw new InvalidOperationException("Invalid search type"),
            };
        }

        private T CreateSearchQuery<T>(Repository repository) where T : SearchQuery
        {
            return SearchType switch
            {
                SearchType.FileContent => (T)(SearchQuery)new FileContentSearchQuery(Search, Pattern, Branch, repository, IsCaseSensitive, IsRegex),
                SearchType.CommitMessage => (T)(SearchQuery)new CommitMessageSearchQuery(Search, Branch, repository, IsCaseSensitive, IsRegex),
                _ => throw new InvalidOperationException("Invalid search type"),
            };
        }
    }
}
