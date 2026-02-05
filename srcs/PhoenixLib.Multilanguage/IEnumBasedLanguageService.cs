// NosEmu
// 


using System;

namespace PhoenixLib.MultiLanguage
{
    /// <summary>
    ///     Permits multi language Key/value based on an enum
    /// </summary>
    public interface IEnumBasedLanguageService<T> : ILanguageService<T>
    where T : Enum
    {
    }
}