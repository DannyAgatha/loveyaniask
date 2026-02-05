using System.Collections.Generic;
using System.Linq;

namespace WingsEmu.Core.Extensions;

public static class Lists
{
    public static List<T> Create<T>(params T[] values) => new(values);

    public static List<T> SetFirst<T>(this List<T> list, T value) where T : class
    {
        T firstValue = list.FirstOrDefault();
        if (firstValue == value)
        {
            return list;
        }

        int index = list.IndexOf(value);
        if (index < 0)
        {
            list.Add(firstValue);
        }
        else
        {
            list[index] = firstValue;
        }

        list[0] = value;

        return list;
    }
}