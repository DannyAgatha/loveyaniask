using System.Collections.Generic;
using System.Linq;

namespace WingsEmu.Core.Extensions;

public static class CollectionExtension
{
    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int size)
    {
        int pos = 0;
        source = source.ToArray();
        while (source.Skip(pos).Any())
        {
            yield return source.Skip(pos).Take(size);
            pos += size;
        }
    }
}