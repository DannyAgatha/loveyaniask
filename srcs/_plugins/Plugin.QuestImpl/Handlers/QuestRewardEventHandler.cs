using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quests;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Quests;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Configurations;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace Plugin.QuestImpl.Handlers
{
    public class QuestRewardEventHandler : IAsyncEventProcessor<QuestRewardEvent>
    {
        private static readonly HashSet<int> SkillsToLearn = new()
        {
            (int)SkillsVnums.BEGINNER_PHYSICAL_STRENGTH, (int)SkillsVnums.BEGINNER_SPEED,
            (int)SkillsVnums.BEGINNER_INTELLIGENCE, (int)SkillsVnums.BEGINNER_HP_RECOVERY
        };

        private static readonly HashSet<int> TarotRewards = new()
        {
            (int)ItemVnums.TAROT_FOOL, (int)ItemVnums.TAROT_MAGICIAN, (int)ItemVnums.TAROT_LOVERS, (int)ItemVnums.TAROT_HERMIT, (int)ItemVnums.TAROT_DEATH,
            (int)ItemVnums.TAROT_DEVIL, (int)ItemVnums.TAROT_TOWER, (int)ItemVnums.TAROT_STAR, (int)ItemVnums.TAROT_MOON, (int)ItemVnums.TAROT_SUN
        };

        private readonly IBuffFactory _buffFactory;
        private readonly ICharacterAlgorithm _characterAlgorithm;
        private readonly IDropRarityConfigurationProvider _dropRarityConfigurationProvider;
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly IGameLanguageService _gameLanguageService;
        private readonly GameMinMaxConfiguration _gameMinMaxConfiguration;
        private readonly IItemsManager _itemsManager;
        private readonly IQuestManager _questManager;
        private readonly QuestsRatesConfiguration _questsRates;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IRankingManager _rankingManager;
        private readonly IReputationConfiguration _reputationConfiguration;
        private readonly ISkillsManager _skillsManager;
        private readonly SoundFlowerConfiguration _soundFlowerConfiguration;
        private readonly IEvtbConfiguration _evtbConfiguration;

        public QuestRewardEventHandler(IGameItemInstanceFactory gameItemInstanceFactory, ICharacterAlgorithm characterAlgorithm, IRandomGenerator randomGenerator, ISkillsManager skillsManager,
            IItemsManager itemsManager, QuestsRatesConfiguration questsRates, GameMinMaxConfiguration gameMinMaxConfiguration, IGameLanguageService gameLanguageService,
            IDropRarityConfigurationProvider dropRarityConfigurationProvider, IQuestManager questManager, IReputationConfiguration reputationConfiguration, IRankingManager rankingManager,
            SoundFlowerConfiguration soundFlowerConfiguration, IBuffFactory buffFactory, IEvtbConfiguration evtbConfiguration)
        {
            _gameItemInstanceFactory = gameItemInstanceFactory;
            _characterAlgorithm = characterAlgorithm;
            _randomGenerator = randomGenerator;
            _skillsManager = skillsManager;
            _itemsManager = itemsManager;
            _questsRates = questsRates;
            _gameMinMaxConfiguration = gameMinMaxConfiguration;
            _gameLanguageService = gameLanguageService;
            _dropRarityConfigurationProvider = dropRarityConfigurationProvider;
            _questManager = questManager;
            _reputationConfiguration = reputationConfiguration;
            _rankingManager = rankingManager;
            _soundFlowerConfiguration = soundFlowerConfiguration;
            _buffFactory = buffFactory;
            _evtbConfiguration = evtbConfiguration;
        }

        public async Task HandleAsync(QuestRewardEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;

            CharacterQuest characterQuest = session.PlayerEntity.GetQuestById(e.QuestId);
            if (characterQuest == null)
            {
                return;
            }

            if (characterQuest.Quest.Prizes.Any())
            {
                TutorialDto qPayScript = _questManager.GetQuestPayScriptByQuestId(characterQuest.QuestId);
                if (!e.ClaimReward && qPayScript != null)
                {
                    return;
                }

                // We only really care about the random reward given to QuestRewardType.RandomReward
                IReadOnlyCollection<CharacterQuestGeneratedReward> rndRewards = GiveRewards(session, characterQuest);
                session.SendQrPacket(characterQuest, rndRewards, _questsRates);
                if (qPayScript != null)
                {
                    session.PlayerEntity.SaveScript(qPayScript.ScriptId, qPayScript.ScriptIndex, TutorialActionType.WAIT_FOR_REWARDS_CLAIM, DateTime.UtcNow);
                }
            }

            if (characterQuest.Quest.QuestType != QuestType.NOTHING)
            {
                session.PlayerEntity.AddDignity(50, _gameMinMaxConfiguration, _gameLanguageService, _reputationConfiguration, _rankingManager.TopReputation);
            }

            if (_soundFlowerConfiguration.WildSoundFlowerQuestVnums.Contains(characterQuest.QuestId))
            {
                int rndBuffVnum = _soundFlowerConfiguration.PossibleBuffs.ElementAt(_randomGenerator.RandomNumber(_soundFlowerConfiguration.PossibleBuffs.Count));

                Buff rndBuff = _buffFactory.CreateBuff(rndBuffVnum, session.PlayerEntity);
                await session.PlayerEntity.AddBuffsAsync(new[] { rndBuff });
            }

            HandleSpecialQuestsRewards(session, characterQuest);
        }

        private IReadOnlyCollection<CharacterQuestGeneratedReward> GiveRewards(IClientSession session, CharacterQuest characterQuest)
        {
            IEnumerable<QuestPrizeDto> prizes = characterQuest.Quest.Prizes;
            if (prizes == null)
            {
                return Array.Empty<CharacterQuestGeneratedReward>();
            }

            List<CharacterQuestGeneratedReward> generatedRewards = new();
            foreach (QuestPrizeDto prize in prizes)
            {
                GameItemInstance itemToAdd;
                
                int eventIncreaseGold = _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_GOLD_EARNED);
                int eventIncreaseExperience = _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_EXPERIENCE_EARNED);
                int eventIncreaseReputation = _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_REPUTATION_EARNED);
                
                double goldMultiplier = eventIncreaseGold >= 1 ? eventIncreaseGold : 1;
                double experienceMultiplier = eventIncreaseExperience >= 1 ? eventIncreaseExperience : 1;
                double reputationMultiplier = eventIncreaseReputation >= 1 ? eventIncreaseReputation : 1;
                
                switch (prize.RewardType)
                {
                    case (byte)QuestRewardType.Gold:
                        session.EmitEventAsync(new GenerateGoldEvent((long)(prize.Data0 * _questsRates.GoldRate * goldMultiplier), true));
                        break;
                    case (byte)QuestRewardType.SecondGold:
                        session.EmitEventAsync(new GenerateGoldEvent((long)(prize.Data0 * _questsRates.BaseGold * _questsRates.GoldRate * goldMultiplier), true));
                        break;
                    case (byte)QuestRewardType.Exp:
                        session.EmitEventAsync(new AddExpEvent((long)(_characterAlgorithm.GetLevelXpPercentage((short)prize.Data0, (short)prize.Data1) * _questsRates.XpRate * experienceMultiplier), LevelType.Level));
                        break;
                    case (byte)QuestRewardType.HeroExpPercentage:
                        session.EmitEventAsync(new AddExpEvent((long)(_characterAlgorithm.GetHeroLevelXpPercentage((short)prize.Data0, (short)prize.Data1) * _questsRates.HeroXpRate * experienceMultiplier), LevelType.Heroic));
                        break;
                    case (byte)QuestRewardType.SetJobLevel:
                        if (session.PlayerEntity.JobLevel < prize.Data0)
                        {
                            session.EmitEvent(new SetLevelEvent
                            {
                                Level = (byte)prize.Data0,
                                LevelType = LevelType.JobLevel
                            });
                        }
                        break;
                    case (byte)QuestRewardType.SetLevel:
                        if (session.PlayerEntity.Level < prize.Data0)
                        {
                            session.EmitEvent(new SetLevelEvent
                            {
                                Level = (byte)prize.Data0,
                                LevelType = LevelType.Level
                            });
                        }
                        break;
                    case (byte)QuestRewardType.SetLevelAndJobLevel:
                        if (session.PlayerEntity.Level < prize.Data0)
                        {
                            session.EmitEvent(new SetLevelEvent()
                            {
                                Level = (byte)prize.Data0,
                                LevelType = LevelType.Level
                            });
                        }
                        
                        if (session.PlayerEntity.JobLevel < prize.Data1)
                        {
                            session.EmitEvent(new SetLevelEvent
                            {
                                Level = (byte)prize.Data1,
                                LevelType = LevelType.JobLevel
                            });
                        }
                        
                        break;
                    case (byte)QuestRewardType.SecondExp:
                        session.EmitEventAsync(new AddExpEvent((long)(prize.Data0 * _questsRates.BaseXp * _questsRates.XpRate * experienceMultiplier), LevelType.Level));
                        break;
                    case (byte)QuestRewardType.JobExp:
                        bool useSp = session.PlayerEntity.UseSp;
                        GameItemInstance specialist = session.PlayerEntity.Specialist;
                        long exp;
                        double jobXpRate = _questsRates.JobXpRate * experienceMultiplier;
                        
                        if (useSp && specialist != null)
                        {
                            if (specialist.IsTrainerSpecialist())
                            {
                                exp = (long)(_characterAlgorithm.GetJobXpPercentage((short)prize.Data0, (short)prize.Data1) * jobXpRate);
                            }
                            else
                            {
                                bool isFunSpecialist = specialist.IsFunSpecialist();
                                exp = (long)(_characterAlgorithm.GetSpecialistJobXpPercentage((short)prize.Data0, (short)prize.Data1, isFunSpecialist) * jobXpRate);
                            }
                        }
                        else
                        {
                            exp = (long)(_characterAlgorithm.GetJobXpPercentage((short)prize.Data0, (short)prize.Data1) * jobXpRate);
                        }
                        session.EmitEventAsync(new AddExpEvent(exp, session.PlayerEntity.UseSp && session.PlayerEntity.Specialist != null ? LevelType.SpJobLevel : LevelType.JobLevel));
                        break;
                    case (byte)QuestRewardType.RandomReward:
                        var possibleRewards = new[] { prize.Data0, prize.Data1, prize.Data2, prize.Data3 }.Where(s => s != -1).ToList();
                        int rndRewardVnum = possibleRewards[_randomGenerator.RandomNumber(possibleRewards.Count)];

                        itemToAdd = _gameItemInstanceFactory.CreateItem(rndRewardVnum, prize.Data4);
                        session.AddNewItemToInventory(itemToAdd, sendGiftIsFull: true);
                        generatedRewards.Add(new CharacterQuestGeneratedReward
                        {
                            ItemVnum = rndRewardVnum,
                            Amount = prize.Data4
                        });
                        break;
                    case (byte)QuestRewardType.AllRewards:
                        foreach (int itemVnum in new[] { prize.Data0, prize.Data1, prize.Data2, prize.Data3 })
                        {
                            if (itemVnum == -1)
                            {
                                continue;
                            }

                            IGameItem item = _itemsManager.GetItem(itemVnum);
                            sbyte randomRarity = _dropRarityConfigurationProvider.GetRandomRarity(item.ItemType);

                            itemToAdd = _gameItemInstanceFactory.CreateItem(itemVnum, 1, 0, randomRarity);
                            session.AddNewItemToInventory(itemToAdd, sendGiftIsFull: true);
                        }

                        break;
                    case (byte)QuestRewardType.Reput:
                        session.EmitEventAsync(new GenerateReputationEvent { Amount = (long)(prize.Data0 * _questsRates.ReputRate * reputationMultiplier), SendMessage = true });
                        break;
                    case (byte)QuestRewardType.ThirdGold:
                        session.EmitEventAsync(new GenerateGoldEvent((long)(prize.Data0 * characterQuest.ObjectiveAmount.Sum(s => s.Value.RequiredAmount) * session.PlayerEntity.Level * _questsRates.GoldRate * goldMultiplier),
                            true));
                        break;
                    case (byte)QuestRewardType.ThirdExp:
                        session.EmitEventAsync(new AddExpEvent((long)(prize.Data0 * characterQuest.ObjectiveAmount.Sum(s => s.Value.RequiredAmount) * session.PlayerEntity.Level * _questsRates.XpRate * experienceMultiplier),
                            LevelType.Level));
                        break;
                    case (byte)QuestRewardType.SecondJobExp:
                        session.EmitEventAsync(new AddExpEvent((long)(prize.Data0 * characterQuest.ObjectiveAmount.Sum(s => s.Value.RequiredAmount) * session.PlayerEntity.Level * _questsRates.JobXpRate * experienceMultiplier),
                            LevelType.JobLevel));
                        break;
                    case (byte)QuestRewardType.Unknow:
                        break;
                    case (byte)QuestRewardType.ItemsDependingOnClass:
                        GameItemInstance itemDependingOnClass;
                        IGameItem gameItem;
                        sbyte randomItemRarity;

                        switch (session.PlayerEntity.Class)
                        {
                            case ClassType.Swordman:
                                gameItem = _itemsManager.GetItem(prize.Data0);
                                if (gameItem == null)
                                {
                                    continue;
                                }

                                randomItemRarity = _dropRarityConfigurationProvider.GetRandomRarity(gameItem.ItemType);

                                itemDependingOnClass = _gameItemInstanceFactory.CreateItem(prize.Data0, prize.Data4, 0, randomItemRarity);
                                break;
                            case ClassType.Archer:
                                gameItem = _itemsManager.GetItem(prize.Data1);
                                if (gameItem == null)
                                {
                                    continue;
                                }

                                randomItemRarity = _dropRarityConfigurationProvider.GetRandomRarity(gameItem.ItemType);

                                itemDependingOnClass = _gameItemInstanceFactory.CreateItem(prize.Data1, prize.Data4, 0, randomItemRarity);
                                break;
                            case ClassType.Magician:
                                gameItem = _itemsManager.GetItem(prize.Data2);
                                if (gameItem == null)
                                {
                                    continue;
                                }

                                randomItemRarity = _dropRarityConfigurationProvider.GetRandomRarity(gameItem.ItemType);

                                itemDependingOnClass = _gameItemInstanceFactory.CreateItem(prize.Data2, prize.Data4, 0, randomItemRarity);
                                break;
                            default:
                                gameItem = _itemsManager.GetItem(prize.Data3);
                                if (gameItem == null)
                                {
                                    continue;
                                }

                                randomItemRarity = _dropRarityConfigurationProvider.GetRandomRarity(gameItem.ItemType);

                                itemDependingOnClass = _gameItemInstanceFactory.CreateItem(prize.Data3, prize.Data4, 0, randomItemRarity);
                                break;
                        }

                        if (itemDependingOnClass == null)
                        {
                            return Array.Empty<CharacterQuestGeneratedReward>();
                        }

                        session.AddNewItemToInventory(itemDependingOnClass, sendGiftIsFull: true);
                        break;
                }
            }

            return generatedRewards;
        }

        private void HandleSpecialQuestsRewards(IClientSession session, CharacterQuest characterQuest)
        {
            switch (characterQuest.QuestId)
            {
                case (short)QuestsVnums.SORAYA_LUNCH_TO_CALVIN:
                    GameItemInstance calvinLunch = _gameItemInstanceFactory.CreateItem((short)ItemVnums.DELICIOUS_LUNCH);
                    session.AddNewItemToInventory(calvinLunch, true, sendGiftIsFull: true);
                    break;
                case (short)QuestsVnums.GIVE_MALCOM_ADVENTURER_SHOES:
                    GameItemInstance malcomShoes = _gameItemInstanceFactory.CreateItem((short)ItemVnums.ADVENTURER_SHOES, 1, 3);
                    malcomShoes.DarkResistance += (short)(malcomShoes.GameItem.DarkResistance * 3);
                    session.AddNewItemToInventory(malcomShoes, true, sendGiftIsFull: true);
                    break;
                case (short)QuestsVnums.CALVIN_ADVENTURER_TRAINING_SKILLS:
                    foreach (SkillDTO ski in _skillsManager.GetSkills())
                    {
                        if (!SkillsToLearn.Contains(ski.Id))
                        {
                            continue;
                        }

                        // Find higher passive already in PlayerEntity
                        CharacterSkill findHigherPassive = session.PlayerEntity.CharacterSkills.Values.FirstOrDefault(x => x.Skill.IsPassiveSkill()
                            && x.Skill.CastId == ski.CastId && x.Skill.Id > ski.Id);

                        if (findHigherPassive != null)
                        {
                            continue;
                        }

                        var passive = new CharacterSkill { SkillVNum = ski.Id };

                        session.PlayerEntity.CharacterSkills[ski.Id] = passive;
                        session.PlayerEntity.Skills.Add(passive);
                    }

                    session.PlayerEntity.ClearSkillCooldowns();
                    session.RefreshPassiveBCards();
                    session.RefreshSkillList();
                    session.RefreshQuicklist();
                    break;
                case (short)QuestsVnums.FORTUNE_TELLER_1:
                case (short)QuestsVnums.FORTUNE_TELLER_2:
                case (short)QuestsVnums.FORTUNE_TELLER_3:
                case (short)QuestsVnums.FORTUNE_TELLER_4:
                case (short)QuestsVnums.FORTUNE_TELLER_5:
                    int randomTarotVnum = TarotRewards.ElementAt(_randomGenerator.RandomNumber(TarotRewards.Count));
                    GameItemInstance randomTarot = _gameItemInstanceFactory.CreateItem((short)randomTarotVnum);
                    session.AddNewItemToInventory(randomTarot, true, sendGiftIsFull: true);
                    break;
            }
        }
    }
}