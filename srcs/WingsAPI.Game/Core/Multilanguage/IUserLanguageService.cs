using PhoenixLib.MultiLanguage;

namespace WingsEmu.Game._i18n;

/// <summary>
///     This will use IGameLanguageService
/// </summary>
public interface IUserLanguageService
{
    /// <summary>
    ///     Will return the string by its Key & <see cref="RegionLanguageType" />
    ///     Used for plugins mainly
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string GetLanguage(string key);

    string GetLanguageFormat(string key, params object[] formatParams);

    /// <summary>
    ///     Will return the string by its key & <see cref="RegionLanguageType" />
    ///     Used for ChickenAPI mainly
    /// </summary>
    /// <param name="key"></param>
    /// <param name="lang"></param>
    /// <returns></returns>
    string GetLanguage(GameDialogKey key);

    /// <summary>
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="formatParams"></param>
    /// <returns></returns>
    string GetLanguageFormat(GameDialogKey dataType, params object[] formatParams);
}