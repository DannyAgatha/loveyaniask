using WingsEmu.DTOs.Buffs;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game._i18n;

public static class GameLanguageServiceExtensions
{
    public static string GetItemName(this IGameLanguageService gameLanguage, IGameItem item, IClientSession session) => gameLanguage.GetLanguage(GameDataType.Item, item.Name, session.UserLanguage);

    public static string GetNpcMonsterName(this IGameLanguageService gameLanguage, IMonsterData npc, IClientSession session) =>
        gameLanguage.GetLanguage(GameDataType.NpcMonster, npc.Name, session.UserLanguage);
    
    public static string GetMapName(this IGameLanguageService gameLanguage, MapDataDTO map, IClientSession session) => gameLanguage.GetLanguage(GameDataType.Map, map.Name, session.UserLanguage);

    public static string GetSkillName(this IGameLanguageService gameLanguage, SkillDTO skill, IClientSession session) => gameLanguage.GetLanguage(GameDataType.Skill, skill.Name, session.UserLanguage);

    public static string GetCardName(this IGameLanguageService gameLanguage, CardDTO card, IClientSession session) => gameLanguage.GetLanguage(GameDataType.Card, card.Name, session.UserLanguage);
}