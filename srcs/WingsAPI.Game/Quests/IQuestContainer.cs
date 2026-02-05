using System;
using System.Collections.Generic;
using WingsEmu.DTOs.Quests;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Quests;

public interface IQuestContainer
{
    public bool HasQuestWithQuestType(QuestType questType);
    public bool HasQuestWithId(int questId);
    public bool HasCompletedScriptByIndex(int scriptId, int scriptIndex);
    public bool HasCompletedQuest(int questId);
    public IEnumerable<CharacterQuest> GetCurrentQuests();
    public IEnumerable<CharacterQuest> GetCompletedQuests();
    public IEnumerable<CharacterQuestDto> GetCompletedPeriodicQuests();
    public IEnumerable<CharacterQuest> GetCurrentQuestsByType(QuestType questType);
    public IEnumerable<CharacterQuest> GetCurrentQuestsByTypes(IReadOnlyCollection<QuestType> questTypes);
    public IEnumerable<CharacterQuestDto> GetQuestsProgress();
    public CharacterQuest GetCurrentQuest(int questId);
    public void AddActiveQuest(CharacterQuest quest);
    public void RemoveActiveQuest(int questId);
    public void AddCompletedQuest(CharacterQuest quest);
    public void RemoveCompletedQuest(int questId);
    public void RemoveCompletedScript(int scriptId, int scriptIndex);
    public void RemoveAllCompletedScripts();
    public void AddCompletedPeriodicQuest(CharacterQuest quest);
    public void ClearCompletedPeriodicQuests();

    public IEnumerable<CompletedScriptsDto> GetCompletedScripts();
    public IEnumerable<CompletedScriptsDto> GetCompletedScriptsByType(TutorialActionType scriptType);
    public void SaveScript(int scriptId, int scriptIndex, TutorialActionType scriptType, DateTime savingDate);
    public CompletedScriptsDto GetLastCompletedScript();
    public CompletedScriptsDto GetLastCompletedScriptByType(TutorialActionType scriptType);


    public void IncreasePendingSoundFlowerQuests();
    public void DecreasePendingSoundFlowerQuests();
    public int GetPendingSoundFlowerQuests();
}