// NosEmu
// 


using System.Linq;

namespace WingsEmu.Game.Extensions;

public static class StringExtensions
{
    #region Methods

    public static string Truncate(this string str, int length) => str.Length > length ? str.Substring(0, length) : str;

    public static string ToUnderscoreCase(this string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
    }

    #endregion
}