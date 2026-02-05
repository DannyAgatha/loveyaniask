using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core.Extensions;
using WingsEmu.Core.Generics;
using WingsEmu.DTOs.Quests;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Quests;

public class BasicQuestContainer : IQuestContainer
{
    private readonly ConcurrentDictionary<int, CharacterQuest> _activeQuests = new();
    private readonly ConcurrentDictionary<int, CharacterQuest> _completedPeriodicQuests = new();
    private readonly ConcurrentDictionary<int, CharacterQuest> _completedQuests = new();
    private readonly ConcurrentDictionary<(int, int), CompletedScriptsDto> _completedScripts = new();
    private readonly ConcurrentDictionary<TutorialActionType, ThreadSafeList<CompletedScriptsDto>> _completedScriptsByType = new();
    private readonly ConcurrentDictionary<QuestType, ThreadSafeList<CharacterQuest>> _questsByQuestType = new();
    private int _soundFlowersPendingToStart;

    public bool HasCompletedScriptByIndex(int scriptId, int scriptIndex) => _completedScripts.ContainsKey((scriptId, scriptIndex));
    public bool HasCompletedQuest(int questId) => _completedQuests.ContainsKey(questId);

    public IEnumerable<CharacterQuest> GetCurrentQuests() => _activeQuests.Values;

    public IEnumerable<CharacterQuest> GetCompletedQuests() => _completedQuests.Values;

    public IEnumerable<CharacterQuestDto> GetCompletedPeriodicQuests() => _completedPeriodicQuests.Values.Cast<CharacterQuestDto>().ToArray();

    public IEnumerable<CharacterQuest> GetCurrentQuestsByType(QuestType questType)
    {
        return _activeQuests.Values.Where(s => s.Quest.QuestType == questType).ToArray();
    }

    public IEnumerable<CharacterQuest> GetCurrentQuestsByTypes(IReadOnlyCollection<QuestType> questTypes)
    {
        return _activeQuests.Values.Where(s => questTypes.Contains(s.Quest.QuestType)).ToArray();
    }

    public IEnumerable<CharacterQuestDto> GetQuestsProgress() => _activeQuests.Values.Cast<CharacterQuestDto>().ToList();

    public CharacterQuest GetCurrentQuest(int questId) => _activeQuests.GetOrDefault(questId);

    public bool HasQuestWithQuestType(QuestType questType) => _questsByQuestType.ContainsKey(questType);

    public bool HasQuestWithId(int questId) => _activeQuests.ContainsKey(questId);

    public void AddActiveQuest(CharacterQuest quest)
    {
        if (_activeQuests.ContainsKey(quest.QuestId))
        {
            return;
        }

        _activeQuests.TryAdd(quest.QuestId, quest);

        if (!_questsByQuestType.TryGetValue(quest.Quest.QuestType, out ThreadSafeList<CharacterQuest> characterQuests))
        {
            characterQuests = new ThreadSafeList<CharacterQuest>();
            _questsByQuestType[quest.Quest.QuestType] = characterQuests;
        }

        characterQuests.Add(quest);
    }

    public void RemoveActiveQuest(int questId)
    {
        if (!_activeQuests.Remove(questId, out CharacterQuest quest))
        {
            return;
        }

        if (!_questsByQuestType.TryGetValue(quest.Quest.QuestType, out ThreadSafeList<CharacterQuest> characterQuests))
        {
            return;
        }

        characterQuests.Remove(quest);
    }

    public void AddCompletedQuest(CharacterQuest quest)
    {
        if (_completedQuests.ContainsKey(quest.QuestId))
        {
            return;
        }

        _completedQuests.TryAdd(quest.QuestId, quest);
    }

    public void RemoveCompletedQuest(int questId) => _completedQuests.TryRemove(questId, out _);

    public void RemoveCompletedScript(int scriptId, int scriptIndex) => _completedScripts.TryRemove((scriptId, scriptIndex), out _);

    public void RemoveAllCompletedScripts() => _completedScripts.Clear();

    public void AddCompletedPeriodicQuest(CharacterQuest quest)
    {
        if (_completedPeriodicQuests.ContainsKey(quest.QuestId))
        {
            return;
        }

        _completedPeriodicQuests.TryAdd(quest.QuestId, quest);
    }

    public void ClearCompletedPeriodicQuests() => _completedPeriodicQuests.Clear();

    public IEnumerable<CompletedScriptsDto> GetCompletedScripts() => _completedScripts.Values.ToArray();

    public IEnumerable<CompletedScriptsDto> GetCompletedScriptsByType(TutorialActionType scriptType)
    {
        if (!_completedScriptsByType.TryGetValue(scriptType, out ThreadSafeList<CompletedScriptsDto> list))
        {
            return Array.Empty<CompletedScriptsDto>();
        }

        return list;
    }

    public void SaveScript(int scriptId, int scriptIndex, TutorialActionType scriptType, DateTime savingDate)
    {
        var completedScript = new CompletedScriptsDto
        {
            ScriptId = scriptId,
            ScriptIndex = scriptIndex,
            CompletedDate = savingDate
        };

        _completedScripts.TryAdd((scriptId, scriptIndex), completedScript);

        if (!_completedScriptsByType.TryGetValue(scriptType, out ThreadSafeList<CompletedScriptsDto> list))
        {
            list = new ThreadSafeList<CompletedScriptsDto>();
            _completedScriptsByType[scriptType] = list;
        }

        list.Add(completedScript);
    }

    public CompletedScriptsDto GetLastCompletedScript()
    {
        if (!_completedScripts.Any())
        {
            return null;
        }

        return _completedScripts.OrderByDescending(s => s.Key.Item1).ThenByDescending(s => s.Key.Item2).FirstOrDefault().Value;
    }

    public CompletedScriptsDto GetLastCompletedScriptByType(TutorialActionType scriptType)
    {
        if (!_completedScriptsByType.ContainsKey(scriptType) || !_completedScriptsByType[scriptType].Any())
        {
            return null;
        }

        return _completedScriptsByType[scriptType].OrderByDescending(s => s.ScriptId).ThenByDescending(s => s.ScriptIndex).FirstOrDefault();
    }

    public void IncreasePendingSoundFlowerQuests() => _soundFlowersPendingToStart++;

    public void DecreasePendingSoundFlowerQuests() => _soundFlowersPendingToStart--;

    public int GetPendingSoundFlowerQuests() => _soundFlowersPendingToStart;
}