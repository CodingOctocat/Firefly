using System;
using System.Collections.Generic;

using Firefly.Common;

namespace Firefly.Extensions;

public static class EnumerableExtensions
{
    public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        int i = 0;

        foreach (var item in source)
        {
            if (predicate(item))
            {
                return i;
            }

            i++;
        }

        return -1;
    }

    public static ObservableRangeCollection<T> ToObservableRangeCollection<T>(this IEnumerable<T> source)
    {
        return [.. source];
    }
}
