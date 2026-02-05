// NosEmu
// 


using System;
using System.Linq;

namespace WingsEmu.Game.Extensions;

public static class EnumExtensions
{
    #region Methods

    public static string ConvertEnumToString<TEnum>(TEnum enumValue) where TEnum : Enum
    {
        string stringValue = enumValue.ToString().ToLower().Replace("_", " ");
        return stringValue;
    }

    #endregion
}