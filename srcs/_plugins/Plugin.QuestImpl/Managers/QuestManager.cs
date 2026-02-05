// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game.Quests;

namespace Plugin.QuestImpl.Managers
{
    public class QuestManager : IQuestManager
    {
        private readonly IExpirableLockService _lockService;
        private readonly ILongKeyCachedRepository<QuestDto> _questCache;
        private readonly Dictionary<int, List<int>> _questlines = new();
        private readonly IKeyValueCache<List<QuestDto>> _questNameCache;
        private readonly Dictionary<int, QuestNpcDto> _questNpcById = new();
        private readonly IResourceLoader<QuestNpcDto> _questNpcResourceLoader;

        private readonly IResourceLoader<QuestDto> _questResourceLoader;
        private readonly Dictionary<int, TutorialDto> _tutorialDataById = new();
        private readonly Dictionary<int, List<TutorialDto>> _tutorialDataByScriptId = new();
        private readonly Dictionary<TutorialActionType, List<TutorialDto>> _tutorialDataByType = new();
        private readonly Dictionary<int, TutorialDto> _tutorialQuestPayScriptByQuestId = new();
        private readonly IResourceLoader<TutorialDto> _tutorialResourceLoader;


        public QuestManager(ILongKeyCachedRepository<QuestDto> questCache, IResourceLoader<QuestDto> questResourceLoader, IResourceLoader<TutorialDto> tutorialResourceLoader,
            IResourceLoader<QuestNpcDto> questNpcResourceLoader, IExpirableLockService lockService, IKeyValueCache<List<QuestDto>> questNameCache)
        {
            _questCache = questCache;
            _questResourceLoader = questResourceLoader;
            _questNpcResourceLoader = questNpcResourceLoader;
            _tutorialResourceLoader = tutorialResourceLoader;
            _lockService = lockService;
            _questNameCache = questNameCache;
        }

        public IReadOnlyCollection<QuestNpcDto> GetNpcBlueAlertQuests() => _questNpcById.Values.Where(s => s.QuestId != null).ToList();
        public QuestNpcDto GetNpcBlueAlertQuestByQuestId(int questId) => _questNpcById.Values.FirstOrDefault(s => s.QuestId == questId);

        public async Task<bool> CanRefreshDailyQuests(long characterId) =>
            await _lockService.TryAddTemporaryLockAsync($"game:locks:quest-daily-refresh:{characterId}", DateTime.UtcNow.Date.AddDays(1));
        
        public async Task<bool> IsDailyQuestExist(Guid masterAccId, int questPackId) =>
            await _lockService.ExistsTemporaryLock($"game:locks:quest-daily-master:{masterAccId}:{questPackId.ToString()}");

        public async Task<bool> IsDailyQuestExist(long characterId, int questPackId) =>
            await _lockService.ExistsTemporaryLock($"game:locks:quest-daily-char:{characterId}:{questPackId.ToString()}");

        public async Task TryAddDailyQuest(Guid masterAccId, int questPackId) =>
            await _lockService.TryAddTemporaryLockAsync($"game:locks:quest-daily-master:{masterAccId}:{questPackId.ToString()}", DateTime.UtcNow.Date.AddDays(1));

        public async Task TryAddDailyQuest(long characterId, int questPackId) =>
            await _lockService.TryAddTemporaryLockAsync($"game:locks:quest-daily-char:{characterId}:{questPackId.ToString()}", DateTime.UtcNow.Date.AddDays(1));

        public bool IsNpcBlueAlertQuest(int questId) => _questNpcById.Values.Any(s => s.QuestId == questId);

        public List<QuestDto> GetQuestByName(string name) => _questNameCache.Get(name);

        public async Task InitializeAsync()
        {
            int questCounter = 0;
            int scriptsCounter = 0;
            int objectivesCounter = 0;
            int prizesCounter = 0;
            int questsNpcCounter = 0;

            IEnumerable<QuestDto> quests = await _questResourceLoader.LoadAsync();
            IEnumerable<TutorialDto> tutorialScripts = await _tutorialResourceLoader.LoadAsync();
            IEnumerable<QuestNpcDto> questsNpc = await _questNpcResourceLoader.LoadAsync();

            var nextQuests = new Dictionary<int, List<int>>();
            foreach (QuestDto questDto in quests)
            {
                if (questDto.NextQuestId == -1)
                {
                    continue;
                }

                if (!nextQuests.ContainsKey(questDto.Id))
                {
                    nextQuests.Add(questDto.Id, new List<int>());
                }

                nextQuests[questDto.Id].Add(questDto.NextQuestId);
            }

            nextQuests = BuildQuestline(nextQuests);
            foreach (QuestDto quest in quests)
            {
                _questlines.TryAdd(quest.Id, nextQuests.ContainsKey(quest.Id) ? nextQuests[quest.Id] : new List<int>());

                _questCache.Set(quest.Id, quest);
                _questNameCache.GetOrSet(quest.Name, () => new List<QuestDto>()).Add(quest);
                if (quest.NextQuestId != -1)
                {
                    if (!nextQuests.ContainsKey(quest.Id))
                    {
                        nextQuests.Add(quest.Id, new List<int>());
                    }

                    nextQuests[quest.Id].Add(quest.NextQuestId);
                }

                questCounter++;
                objectivesCounter += quest.Objectives.Count;
                prizesCounter += quest.Prizes.Count;
            }

            foreach (TutorialDto tutorialDto in tutorialScripts)
            {
                if (!_tutorialDataByType.ContainsKey(tutorialDto.Type))
                {
                    _tutorialDataByType.Add(tutorialDto.Type, new List<TutorialDto>());
                }

                if (!_tutorialDataByScriptId.ContainsKey(tutorialDto.ScriptId))
                {
                    _tutorialDataByScriptId.Add(tutorialDto.ScriptId, new List<TutorialDto>());
                }

                if (tutorialDto.Type == TutorialActionType.WAIT_FOR_REWARDS_CLAIM)
                {
                    _tutorialQuestPayScriptByQuestId.Add(tutorialDto.Data, tutorialDto);
                }

                _tutorialDataById.TryAdd(tutorialDto.Id, tutorialDto);
                _tutorialDataByType[tutorialDto.Type].Add(tutorialDto);
                _tutorialDataByScriptId[tutorialDto.ScriptId].Add(tutorialDto);
                scriptsCounter++;
            }

            foreach (QuestNpcDto questNpcDto in questsNpc)
            {
                _questNpcById.TryAdd(questNpcDto.Id, questNpcDto);
                questsNpcCounter++;
            }

            Log.Info($"[RESOURCES] Loaded {questCounter} quests.");
            Log.Info($"[RESOURCES] Loaded {objectivesCounter} quests objectives.");
            Log.Info($"[RESOURCES] Loaded {prizesCounter} quests rewards.");
            Log.Info($"[RESOURCES] Loaded {scriptsCounter} tutorial scripts.");
            Log.Info($"[RESOURCES] Loaded {questsNpcCounter} NPC quests.");
        }

        public QuestDto GetQuestById(int questId) => _questCache.Get(questId);

        public IReadOnlyCollection<TutorialDto> GetScriptsTutorialByType(TutorialActionType type) => _tutorialDataByType.ContainsKey(type) ? _tutorialDataByType[type] : Array.Empty<TutorialDto>();

        public IReadOnlyCollection<TutorialDto> GetScriptsTutorialByScriptId(int scriptId) =>
            _tutorialDataByScriptId.ContainsKey(scriptId) ? _tutorialDataByScriptId[scriptId] : Array.Empty<TutorialDto>();

        public IReadOnlyCollection<TutorialDto> GetScriptsTutorial() => _tutorialDataById.Values;

        public IReadOnlyCollection<TutorialDto> GetScriptsTutorialUntilIndex(int scriptId, int scriptIndex) => _tutorialDataById.Values
            .Where(s => s.ScriptId < scriptId || s.ScriptId == scriptId && s.ScriptIndex <= scriptIndex)
            .OrderBy(s => s.ScriptId).ThenBy(s => s.ScriptIndex).ToList();

        public IReadOnlyCollection<int> GetQuestlines(int questId) => _questlines.ContainsKey(questId) ? _questlines[questId] : Array.Empty<int>();

        public TutorialDto GetQuestPayScriptByQuestId(int questId) => _tutorialQuestPayScriptByQuestId.GetOrDefault(questId);

        public TutorialDto GetScriptTutorialById(int scriptId) => _tutorialDataById.ContainsKey(scriptId) ? _tutorialDataById[scriptId] : null;
        public TutorialDto GetScriptTutorialByIndex(int scriptId, int index) => _tutorialDataById.FirstOrDefault(s => s.Value.ScriptId == scriptId && s.Value.ScriptIndex == index).Value;

        public TutorialDto GetFirstScriptFromIdByType(int scriptId, TutorialActionType type) =>
            _tutorialDataByType.ContainsKey(type) ? _tutorialDataByType[type].FirstOrDefault(s => s.Id > scriptId) : null;

        public QuestNpcDto GetQuestNpcByScriptId(int scriptId) => _questNpcById.FirstOrDefault(s => s.Value.StartingScript == scriptId).Value;

        private static Dictionary<int, List<int>> BuildQuestline(IDictionary<int, List<int>> quests)
        {
            bool built = true;
            foreach ((int questId, List<int> nextQuestIds) in quests.ToList())
            {
                if (!quests.ContainsKey(questId))
                {
                    continue;
                }

                foreach (int nextQuestId in nextQuestIds.ToList())
                {
                    if (!quests.ContainsKey(nextQuestId))
                    {
                        continue;
                    }

                    built = false;
                    quests[questId].AddRange(quests[nextQuestId]);
                    quests.Remove(nextQuestId);
                }
            }

            if (!built)
            {
                return BuildQuestline(quests);
            }

            var invertedDictionary = new Dictionary<int, List<int>>();
            foreach ((int questId, List<int> nextQuestIds) in quests)
            {
                foreach (int nextQuestId in nextQuestIds)
                {
                    if (!invertedDictionary.ContainsKey(nextQuestId))
                    {
                        invertedDictionary.Add(nextQuestId, new List<int>());
                    }

                    invertedDictionary[nextQuestId].Add(questId);
                }

                invertedDictionary.Add(questId, new List<int> { questId });
            }

            return invertedDictionary;
        }
    }
}