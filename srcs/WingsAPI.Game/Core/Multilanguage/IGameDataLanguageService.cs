using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.MultiLanguage;

namespace WingsEmu.Game._i18n;

public interface IGameDataLanguageService
{
    /// <summary>
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="dataName"></param>
    /// <param name="lang"></param>
    /// <returns></returns>
    string GetLanguage(GameDataType dataType, string dataName, RegionLanguageType lang);

    Dictionary<string, string> GetDataTranslations(GameDataType dataType, RegionLanguageType lang);

    /// <summary>
    ///     Reload
    /// </summary>
    /// <returns></returns>
    Task Reload(bool isFullReload = false);
}