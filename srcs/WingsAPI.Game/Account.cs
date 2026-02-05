// NosEmu
// 


using System;
using System.Collections.Generic;
using PhoenixLib.MultiLanguage;
using WingsAPI.Data.Account;
using WingsEmu.DTOs.Account;

namespace WingsEmu.Game;

public static class AccountExtensions
{
    public static RegionLanguageType ToRegionLanguageType(this AccountLanguage lang)
    {
        return lang switch
        {
            AccountLanguage.EN => RegionLanguageType.EN,
            AccountLanguage.FR => RegionLanguageType.FR,
            AccountLanguage.DE => RegionLanguageType.DE,
            AccountLanguage.PL => RegionLanguageType.PL,
            AccountLanguage.IT => RegionLanguageType.IT,
            AccountLanguage.ES => RegionLanguageType.ES,
            AccountLanguage.CZ => RegionLanguageType.CZ,
            AccountLanguage.TR => RegionLanguageType.TR,
            _ => throw new ArgumentOutOfRangeException(nameof(lang), lang, null)
        };
    }

    public static AccountLanguage ToAccountLanguage(this RegionLanguageType lang)
    {
        return lang switch
        {
            RegionLanguageType.EN => AccountLanguage.EN,
            RegionLanguageType.FR => AccountLanguage.FR,
            RegionLanguageType.DE => AccountLanguage.DE,
            RegionLanguageType.PL => AccountLanguage.PL,
            RegionLanguageType.IT => AccountLanguage.IT,
            RegionLanguageType.ES => AccountLanguage.ES,
            RegionLanguageType.CZ => AccountLanguage.CZ,
            RegionLanguageType.TR => AccountLanguage.TR,
            _ => throw new ArgumentOutOfRangeException(nameof(lang), lang, null)
        };
    }
}

public class Account : AccountDTO
{
    public List<AccountPenaltyDto> Logs { get; set; } = new();

    public void ChangeLanguage(RegionLanguageType lang)
    {
        Language = lang.ToAccountLanguage();
        LangChanged?.Invoke(this, lang);
    }

    public event EventHandler<RegionLanguageType> LangChanged;
}