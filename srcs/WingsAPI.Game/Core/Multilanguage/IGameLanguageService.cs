// NosEmu
// 


using System;
using PhoenixLib.MultiLanguage;

namespace WingsEmu.Game._i18n;

public interface IGameLanguageService : IGameDataLanguageService
{
    /// <summary>
    ///     Will return the string by its Key & <see cref="RegionLanguageType" />
    ///     Used for plugins mainly
    /// </summary>
    /// <param name="key"></param>
    /// <param name="lang"></param>
    /// <returns></returns>
    string GetLanguage(string key, RegionLanguageType lang);

    string GetLanguageFormat(string key, RegionLanguageType lang, params object[] formatParams);

    /// <summary>
    ///     Will return the string by its key & <see cref="RegionLanguageType" />
    ///     Used for ChickenAPI mainly
    /// </summary>
    /// <param name="key"></param>
    /// <param name="lang"></param>
    /// <returns></returns>
    string GetLanguage<T>(T key, RegionLanguageType lang) where T : Enum;

    /// <summary>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="lang"></param>
    /// <param name="formatParams"></param>
    /// <returns></returns>
    string GetLanguageFormat<T>(T key, RegionLanguageType lang, params object[] formatParams) where T : Enum;
}