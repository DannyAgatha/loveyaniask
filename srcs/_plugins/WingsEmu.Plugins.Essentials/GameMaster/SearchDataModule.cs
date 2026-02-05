using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Quests;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.Essentials.Administrator;

[Name("Administrator")]
[Description("Module related to Administrator commands.")]
[RequireAuthority(AuthorityType.GM)]
public class SearchDataModule : SaltyModuleBase
{
    private readonly ICardsManager _cardManager;
    private readonly IItemsManager _itemManager;
    private readonly IGameLanguageService _language;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IQuestManager _questManager;
    private readonly ISkillsManager _skillManager;
    private readonly IMapManager _mapManager;

    public SearchDataModule(INpcMonsterManager npcMonsterManager, IGameLanguageService language, IItemsManager itemManager, ISkillsManager skillManager, ICardsManager cardManager,
        IQuestManager questManager, IMapManager mapManager)
    {
        _npcMonsterManager = npcMonsterManager;
        _language = language;
        _itemManager = itemManager;
        _skillManager = skillManager;
        _cardManager = cardManager;
        _questManager = questManager;
        _mapManager = mapManager;
    }

    private void GetMatchingNames(GameDataType dataType, IReadOnlyDictionary<string, string> dataNames, string name)
    {
        List<KeyValuePair<int, string>> idName = new();
        switch (dataType)
        {
            case GameDataType.Item:
                idName = dataNames
                    .Where(s => s.Value.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                    .SelectMany(s => _itemManager.GetItem(s.Key))
                    .Select(s => new KeyValuePair<int, string>(s.Id, dataNames[s.Name]))
                    .OrderBy(s => s.Key)
                    .ToList();
                break;
            case GameDataType.NpcMonster:
                idName = dataNames
                    .Where(s => s.Value.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                    .SelectMany(s => _npcMonsterManager.GetNpc(s.Key))
                    .Select(s => new KeyValuePair<int, string>(s.MonsterVNum, dataNames[s.Name]))
                    .OrderBy(s => s.Key)
                    .ToList();
                break;
            case GameDataType.Skill:
                idName = dataNames
                    .Where(s => s.Value.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                    .SelectMany(s => _skillManager.GetSkill(s.Key))
                    .Select(s => new KeyValuePair<int, string>(s.Id, dataNames[s.Name]))
                    .OrderBy(s => s.Key)
                    .ToList();
                break;
            case GameDataType.Card:
                idName = dataNames
                    .Where(s => s.Value.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                    .SelectMany(s => _cardManager.GetCardByName(s.Key))
                    .Select(s => new KeyValuePair<int, string>(s.Id, dataNames[s.Name]))
                    .OrderBy(s => s.Key)
                    .ToList();
                break;
            case GameDataType.QuestName:
                idName = dataNames
                    .Where(s => s.Value.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                    .SelectMany(s => _questManager.GetQuestByName(s.Key))
                    .Select(s => new KeyValuePair<int, string>(s.Id, dataNames[s.Name]))
                    .OrderBy(s => s.Key)
                    .ToList();
                break;
            case GameDataType.Map:
                idName = dataNames
                    .Where(s => s.Value.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                    .SelectMany(s => _mapManager.GetMapByName(s.Key))
                    .Select(s => new KeyValuePair<int, string>(s.Id, dataNames[s.Name]))
                    .OrderBy(s => s.Key)
                    .ToList();
                break;
        }

        foreach ((int key, string value) in idName)
        {
            Context.Player.SendChatMessage($"[{key}] {value}", ChatMessageColorType.Green);
        }
    }

    [Command("searchitem", "findItem")]
    public async Task<SaltyCommandResult> SearchItem([Remainder] string search)
    {
        Dictionary<string, string> languagesAsync = _language.GetDataTranslations(GameDataType.Item, Context.Player.UserLanguage);

        Context.Player.SendChatMessage($"[SEARCH_ITEM] : {search}", ChatMessageColorType.Green);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        GetMatchingNames(GameDataType.Item, languagesAsync, search);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        return new SaltyCommandResult(true);
    }

    [Command("searchmonster", "findMonster", "searchmob")]
    public async Task<SaltyCommandResult> SearchMonster([Remainder] string search)
    {
        Dictionary<string, string> languagesAsync = _language.GetDataTranslations(GameDataType.NpcMonster, Context.Player.UserLanguage);
        Context.Player.SendChatMessage($"[SEARCH_MONSTER] : {search}", ChatMessageColorType.Green);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        GetMatchingNames(GameDataType.NpcMonster, languagesAsync, search);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        return new SaltyCommandResult(true);
    }


    [Command("searchSkill", "findSkill")]
    public async Task<SaltyCommandResult> SearchSkill([Remainder] string search)
    {
        Dictionary<string, string> tmp = _language.GetDataTranslations(GameDataType.Skill, Context.Player.UserLanguage);
        Context.Player.SendChatMessage($"[SEARCH_SKILL] : {search}", ChatMessageColorType.Green);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);

        GetMatchingNames(GameDataType.Skill, tmp, search);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        return new SaltyCommandResult(true);
    }

    [Command("searchBuff", "findBuff")]
    public async Task<SaltyCommandResult> SearchBuff([Remainder] string search)
    {
        Dictionary<string, string> tmp = _language.GetDataTranslations(GameDataType.Card, Context.Player.UserLanguage);
        Context.Player.SendChatMessage($"[SEARCH_BUFF] : {search}", ChatMessageColorType.Green);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);

        GetMatchingNames(GameDataType.Card, tmp, search);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        return new SaltyCommandResult(true);
    }

    [Command("searchQuest", "findQuests")]
    public async Task<SaltyCommandResult> SearchQuests([Remainder] string search)
    {
        Dictionary<string, string> tmp = _language.GetDataTranslations(GameDataType.QuestName, Context.Player.UserLanguage);
        Context.Player.SendChatMessage($"[SEARCH_QUESTS] : {search}", ChatMessageColorType.Green);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        GetMatchingNames(GameDataType.QuestName, tmp, search);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        return new SaltyCommandResult(true);
    }

    [Command("searchMap", "findMap")]
    public async Task<SaltyCommandResult> SearchMap([Remainder] string search)
    {
        Dictionary<string, string> tmp = _language.GetDataTranslations(GameDataType.Map, Context.Player.UserLanguage);
        Context.Player.SendChatMessage($"[SEARCH_MAP] : {search}", ChatMessageColorType.Green);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        GetMatchingNames(GameDataType.Map, tmp, search);
        Context.Player.SendChatMessage("===============================", ChatMessageColorType.Green);
        return new SaltyCommandResult(true);
    }
}