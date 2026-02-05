// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.DTOs.Quests;

namespace WingsEmu.Game.Quests;

public interface IQuestManager
{
    Task InitializeAsync();
    QuestDto GetQuestById(int questId);
    IReadOnlyCollection<TutorialDto> GetScriptsTutorialByType(TutorialActionType type);
    IReadOnlyCollection<TutorialDto> GetScriptsTutorialByScriptId(int scriptId);
    IReadOnlyCollection<TutorialDto> GetScriptsTutorial();
    IReadOnlyCollection<TutorialDto> GetScriptsTutorialUntilIndex(int scriptId, int scriptIndex);
    IReadOnlyCollection<int> GetQuestlines(int questId);
    TutorialDto GetQuestPayScriptByQuestId(int questId);
    TutorialDto GetScriptTutorialById(int scriptId);
    TutorialDto GetScriptTutorialByIndex(int scriptId, int index);
    TutorialDto GetFirstScriptFromIdByType(int scriptId, TutorialActionType type);
    QuestNpcDto GetQuestNpcByScriptId(int scriptId);
    IReadOnlyCollection<QuestNpcDto> GetNpcBlueAlertQuests();
    QuestNpcDto GetNpcBlueAlertQuestByQuestId(int questId);
    Task<bool> CanRefreshDailyQuests(long characterId);
    Task<bool> IsDailyQuestExist(Guid masterAccId, int questPackId);
    Task<bool> IsDailyQuestExist(long characterId, int questPackId);
    Task TryAddDailyQuest(Guid masterAccId, int questPackId);
    Task TryAddDailyQuest(long characterId, int questPackId);
    List<QuestDto> GetQuestByName(string name);
    bool IsNpcBlueAlertQuest(int questId);
}