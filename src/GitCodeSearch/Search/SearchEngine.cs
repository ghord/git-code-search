using GitCodeSearch.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GitCodeSearch.Search
{
    public class SearchEngine(string Search, ObservableCollection<ISearchResult> Results, Action<string?> OnRepositorySearch)
    {
        public SearchType SearchType { get; set; } = Settings.Current.SearchType;
        public string Pattern { get; set; } = Settings.Current.Pattern;
        public string? Branch { get; set; } = Settings.Current.Branch;
        public bool IsCaseSensitive { get; set; } = Settings.Current.IsCaseSensitive;
        public bool IsRegex { get; set; } = Settings.Current.IsRegex;

        /// <summary>
        /// Performs an asynchronous search across all repositories.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task SearchAsync(CancellationToken cancellationToken)
        {
            foreach (var repository in Settings.Current.GitRepositores.OrderByDescending(GitHelper.IsActiveRepository))
            {
                OnRepositorySearch.Invoke(repository.Path);

                if (!GitHelper.IsActiveRepository(repository) && !Settings.Current.ShowInactiveRepositoriesInSearchResult)
                {
                    continue;
                }

                var query = CreateSearchQuery(repository);

                if (GitHelper.IsActiveRepository(repository))
                {
                    await foreach (var result in CreateSearch(query).SearchAsync(cancellationToken))
                    {
                        Results.Add(result);
                    }
                }
                else
                {
                    int count = 0;
                    await foreach (var result in CreateSearch(query).SearchAsync(cancellationToken))
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
                        Results.Add(new InactiveRepositorySearchResult(query, count));
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            OnRepositorySearch.Invoke(null);
        }

        public async Task SearchRepositoryAsync(GitRepository repository, int index, CancellationToken cancellationToken)
        {
            var query = CreateSearchQuery(repository);

            bool first = true;
            await foreach (var result in CreateSearch(query).SearchAsync(cancellationToken))
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

        private ISearch CreateSearch(SearchQuery searchQuery)
        {
            return SearchType switch
            {
                SearchType.FileContent => new FileContentSearch(searchQuery),
                SearchType.CommitMessage => new CommitMessageSearch(searchQuery),
                _ => throw new InvalidOperationException("Invalid search type"),
            };
        }

        private SearchQuery CreateSearchQuery(GitRepository repository)
        {
            return SearchType switch
            {
                SearchType.FileContent => new FileContentSearchQuery(Search, Pattern, Branch, repository, IsCaseSensitive, IsRegex),
                SearchType.CommitMessage => new CommitMessageSearchQuery(Search, Branch, repository, IsCaseSensitive, IsRegex),
                _ => throw new InvalidOperationException("Invalid search type"),
            };
        }

    }
}
