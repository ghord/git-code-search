using System;
using System.Collections.Generic;

namespace GitCodeSearch.Utilities;

internal static class EnumerableExtensions
{
    public static int FindIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
    {
        for (int index = 0; index < list.Count; index++)
        {
            if (predicate(list[index]))
                return index;
        }

        return -1;
    }
}
