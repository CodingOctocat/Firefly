using System.Collections.Generic;

namespace Firefly.Extensions;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> self, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            self.Add(item);
        }
    }
}
