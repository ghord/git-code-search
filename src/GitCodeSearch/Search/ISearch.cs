using System.Collections.Generic;
using System.Threading;

namespace GitCodeSearch.Search
{
    public interface ISearch
    {
        IAsyncEnumerable<ISearchResult> SearchAsync(CancellationToken cancellationToken = default);
    }
}
