using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using Plugin.FamilyImpl.Achievements;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsAPI.Packets.Enums.Chat;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.SubClass;
using WingsEmu.Game.Families;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleEndEventHandler : IAsyncEventProcessor<RainbowBattleEndEvent>
    {
        private readonly IExpirableLockService _expirableLockService;
        private readonly IFamilyAchievementManager _familyAchievementManager;
        private readonly IFamilyMissionManager _familyMissionManager;
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;
        private readonly BattlePassQuestConfiguration _battlePassQuestConfiguration;
        private readonly IGameLanguageService _languageService;
        private readonly RainbowBattleRewardsConfiguration _rainbowBattleRewardsConfiguration;
        private readonly IRandomGenerator _randomGenerator;

        public RainbowBattleEndEventHandler(IFamilyMissionManager familyMissionManager, RainbowBattleConfiguration rainbowBattleConfiguration,
            IGameItemInstanceFactory gameItemInstanceFactory, IFamilyAchievementManager familyAchievementManager, IExpirableLockService expirableLockService,
            BattlePassQuestConfiguration battlePassQuestConfiguration, IGameLanguageService languageService, RainbowBattleRewardsConfiguration rainbowBattleRewardsConfiguration, IRandomGenerator randomGenerator)
        {
            _familyMissionManager = familyMissionManager;
            _rainbowBattleConfiguration = rainbowBattleConfiguration;
            _gameItemInstanceFactory = gameItemInstanceFactory;
            _familyAchievementManager = familyAchievementManager;
            _expirableLockService = expirableLockService;
            _battlePassQuestConfiguration = battlePassQuestConfiguration;
            _languageService = languageService;
            _rainbowBattleRewardsConfiguration = rainbowBattleRewardsConfiguration;
            _randomGenerator = randomGenerator;
        }

        public async Task HandleAsync(RainbowBattleEndEvent e, CancellationToken cancellation)
        {
            RainbowBattleParty rainbowBattleParty = e.RainbowBattleParty;
            rainbowBattleParty.FinishTime = DateTime.UtcNow.AddSeconds(15);

            RainbowBattleTeamType? winnerTeam = null;
            
            if ((rainbowBattleParty.RedTeam.Any() && rainbowBattleParty.RedTeam[0].PlayerEntity.RainbowBattleComponent.IsBattleEnded) ||
                (rainbowBattleParty.BlueTeam.Any() && rainbowBattleParty.BlueTeam[0].PlayerEntity.RainbowBattleComponent.IsBattleEnded))
            {
                return;
            }
            
            foreach (IClientSession session in rainbowBattleParty.RedTeam.Concat(rainbowBattleParty.BlueTeam))
            {
                session.PlayerEntity.RainbowBattleComponent.IsBattleEnded = true;
            }
            
            if (rainbowBattleParty.BluePoints >= 100)
            {
                winnerTeam = RainbowBattleTeamType.Blue;
            }
            else if (rainbowBattleParty.RedPoints >= 100)
            {
                winnerTeam = RainbowBattleTeamType.Red;
            }
            else
            {
                if (rainbowBattleParty.BlueTeam.Count == 0)
                {
                    winnerTeam = RainbowBattleTeamType.Red;
                }
                else if (rainbowBattleParty.RedTeam.Count == 0)
                {
                    winnerTeam = RainbowBattleTeamType.Blue;
                }
                else if (rainbowBattleParty.BluePoints > rainbowBattleParty.RedPoints)
                {
                    winnerTeam = RainbowBattleTeamType.Blue;
                }
                else if (rainbowBattleParty.RedPoints > rainbowBattleParty.BluePoints)
                {
                    winnerTeam = RainbowBattleTeamType.Red;
                }
            }
            
            if (winnerTeam.HasValue)
            {
                RainbowBattleTeamType winningTeam = winnerTeam.Value;
                RainbowBattleTeamType losingTeam = winningTeam == RainbowBattleTeamType.Red ? RainbowBattleTeamType.Blue : RainbowBattleTeamType.Red;

                if (winningTeam == RainbowBattleTeamType.Red)
                {
                    await ProcessWin(rainbowBattleParty, RainbowBattleTeamType.Red);
                    await ProcessLose(rainbowBattleParty, RainbowBattleTeamType.Blue);
                }
                else
                {
                    await ProcessWin(rainbowBattleParty, RainbowBattleTeamType.Blue);
                    await ProcessLose(rainbowBattleParty, RainbowBattleTeamType.Red);
                }
                
                IClientSession winningDummy = winningTeam == RainbowBattleTeamType.Red
                    ? rainbowBattleParty.RedTeam.Count > 0 ? rainbowBattleParty.RedTeam[0] : null
                    : rainbowBattleParty.BlueTeam.Count > 0 ? rainbowBattleParty.BlueTeam[0] : null;

                IClientSession losingDummy = losingTeam == RainbowBattleTeamType.Red
                    ? rainbowBattleParty.RedTeam.Count > 0 ? rainbowBattleParty.RedTeam[0] : null
                    : rainbowBattleParty.BlueTeam.Count > 0 ? rainbowBattleParty.BlueTeam[0] : null;

                if (winningDummy != null)
                {
                    await winningDummy.EmitEventAsync(new RainbowBattleWonEvent
                    {
                        Id = rainbowBattleParty.Id,
                        Players = (winningTeam == RainbowBattleTeamType.Red
                            ? rainbowBattleParty.RedTeam
                            : rainbowBattleParty.BlueTeam).Select(x => x.PlayerEntity.Id).ToArray()
                    });
                }

                if (losingDummy != null)
                {
                    await losingDummy.EmitEventAsync(new RainbowBattleLoseEvent
                    {
                        Id = rainbowBattleParty.Id,
                        Players = (losingTeam == RainbowBattleTeamType.Red
                            ? rainbowBattleParty.RedTeam
                            : rainbowBattleParty.BlueTeam).Select(x => x.PlayerEntity.Id).ToArray()
                    });
                }
            }
            else
            {
                await ProcessLose(rainbowBattleParty, RainbowBattleTeamType.Blue);
                await ProcessLose(rainbowBattleParty, RainbowBattleTeamType.Red);

                IClientSession dummy = rainbowBattleParty.RedTeam.Count > 0 ? rainbowBattleParty.RedTeam[0] : null;
                if (dummy != null)
                {
                    await dummy.EmitEventAsync(new RainbowBattleTieEvent
                    {
                        RedTeam = rainbowBattleParty.RedTeam.Select(x => x.PlayerEntity.Id).ToArray(),
                        BlueTeam = rainbowBattleParty.BlueTeam.Select(x => x.PlayerEntity.Id).ToArray()
                    });
                }
            }
        }

        private async Task ProcessWin(RainbowBattleParty rainbowBattleParty, RainbowBattleTeamType team)
        {
            IReadOnlyList<IClientSession> members = team == RainbowBattleTeamType.Red ? rainbowBattleParty.RedTeam : rainbowBattleParty.BlueTeam;
            string removeClock = RainbowBattleExtensions.GenerateRainbowTime(RainbowTimeType.End);

            short neededPoints = _rainbowBattleConfiguration.NeededActivityPoints;

            foreach (IClientSession member in members)
            {
                member.SendMsgi(MessageType.Default, Game18NConstString.WonRainbowBattle);
                member.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.WonRainbowBattle);
                member.SendEmptyRaidBoss();
                member.SendPacket(removeClock);
                
                // if (member.PlayerEntity.RainbowBattleComponent.ActivityPoints < neededPoints)
                // {
                //     member.SendChatMessage(member.GetLanguage(GameDialogKey.RAINBOW_BATTLE_CHATMESSAGE_NOT_ENOUGH_ACTIVITY_POINTS), ChatMessageColorType.Red);
                //     continue;
                // }

                if (member.PlayerEntity.RainbowBattleLeaverBusterDto is { RewardPenalty: > 0 })
                {
                    member.PlayerEntity.RainbowBattleLeaverBusterDto.RewardPenalty -= 1;

                    if (member.PlayerEntity.RainbowBattleLeaverBusterDto.RewardPenalty == 0)
                    {
                        member.SendChatMessageNoPlayer(member.GetLanguage(GameDialogKey.RAINBOW_BATTLE_CHATMESSAGE_PENALTY_NEXT_REWARD), ChatMessageColorType.IntenseRed);
                    }
                    else
                    {
                        member.SendChatMessageNoPlayer(member.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_CHATMESSAGE_PENALTY_LEFT,
                            member.PlayerEntity.RainbowBattleLeaverBusterDto.RewardPenalty), ChatMessageColorType.IntenseRed);
                    }

                    continue;
                }

                // Ajuste para los ganadores
                bool isLowBracket = member.PlayerEntity.Level is >= 30 and <= 84;
                await ProcessRewards(member, isWinner: true, isLowBracket: isLowBracket);

                int baseExperience = member.PlayerEntity.SubClass.IsPvpSubClass() ? 2000 : member.PlayerEntity.SubClass.IsPvpAndPveSubClass() ? 1000 : 0;
                member.AddTierExperience(baseExperience, _languageService, false);

                await member.EmitEventAsync(new GenerateReputationEvent
                {
                    Amount = _rainbowBattleConfiguration.ReputationMultiplier * member.PlayerEntity.Level,
                    SendMessage = true
                });

                await member.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.CompleteXRainbowBattle));
                ProcessFamilyWinMission(member);
                await ProcessFamilyAchievement(member, true);
                ProcessFamilyMission(member);
            }
        }

        private async Task ProcessLose(RainbowBattleParty rainbowBattleParty, RainbowBattleTeamType team)
        {
            IReadOnlyList<IClientSession> members = team == RainbowBattleTeamType.Red ? rainbowBattleParty.RedTeam : rainbowBattleParty.BlueTeam;
            string removeClock = RainbowBattleExtensions.GenerateRainbowTime(RainbowTimeType.End);

            short neededPoints = _rainbowBattleConfiguration.NeededActivityPoints;

            foreach (IClientSession member in members)
            {
                member.SendMsgi(MessageType.Default, Game18NConstString.LostRainbowBattle);
                member.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.LostRainbowBattle);
                member.SendRaidUiPacket(RaidType.Cuby, RaidWindowType.MISSION_FAIL);
                member.SendPacket(removeClock);

                // if (member.PlayerEntity.RainbowBattleComponent.ActivityPoints < neededPoints)
                // {
                //     member.SendChatMessage(member.GetLanguage(GameDialogKey.RAINBOW_BATTLE_CHATMESSAGE_NOT_ENOUGH_ACTIVITY_POINTS), ChatMessageColorType.Red);
                //     continue;
                // }

                if (member.PlayerEntity.RainbowBattleLeaverBusterDto is { RewardPenalty: > 0 })
                {
                    member.PlayerEntity.RainbowBattleLeaverBusterDto.RewardPenalty -= 1;

                    if (member.PlayerEntity.RainbowBattleLeaverBusterDto.RewardPenalty == 0)
                    {
                        member.SendChatMessageNoPlayer(member.GetLanguage(GameDialogKey.RAINBOW_BATTLE_CHATMESSAGE_PENALTY_NEXT_REWARD), ChatMessageColorType.IntenseRed);
                    }
                    else
                    {
                        member.SendChatMessageNoPlayer(member.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_CHATMESSAGE_PENALTY_LEFT,
                            member.PlayerEntity.RainbowBattleLeaverBusterDto.RewardPenalty), ChatMessageColorType.IntenseRed);
                    }

                    continue;
                }
                
                bool isLowBracket = member.PlayerEntity.Level is >= 30 and <= 84;
                await ProcessRewards(member, isWinner: false, isLowBracket: isLowBracket);

                int baseExperience = member.PlayerEntity.SubClass.IsPvpSubClass() ? 1000 : member.PlayerEntity.SubClass.IsPvpAndPveSubClass() ? 500 : 0;
                member.AddTierExperience(baseExperience, _languageService, false);

                await ProcessFamilyAchievement(member, false);
                ProcessFamilyMission(member);
            }
        }

        private async Task ProcessFamilyAchievement(IClientSession session, bool isWin)
        {
            if (!session.PlayerEntity.IsInFamily())
            {
                return;
            }

            IFamily family = session.PlayerEntity.Family;

            if (await _expirableLockService.TryAddTemporaryLockAsync($"game:locks:family:rainbowbattle:character:{session.PlayerEntity.Id}", DateTime.UtcNow.Date.AddDays(1)))
            {
                _familyAchievementManager.IncrementFamilyAchievement(family.Id, (short)FamilyAchievementsVnum.COMPLETE_10_RAINBOW_BATTLE);

                if (isWin)
                {
                    family.CurrentDayRankStat.RainbowBattlePoints++;
                }
            }

            if (!isWin)
            {
                return;
            }

            if (!await _expirableLockService.TryAddTemporaryLockAsync($"game:locks:family:rainbowbattle-win:character:{session.PlayerEntity.Id}", DateTime.UtcNow.Date.AddDays(1)))
            {
                return;
            }

            _familyAchievementManager.IncrementFamilyAchievement(family.Id, (short)FamilyAchievementsVnum.WIN_5_RAINBOW_BATTLE);
        }

        private async Task ProcessRewards(IClientSession session, bool isWinner, bool isLowBracket)
        {
            List<RainbowBattleReward> rewards;
            
            if (isWinner)
            {
                rewards = isLowBracket ? _rainbowBattleRewardsConfiguration.LowBracketRewards : _rainbowBattleRewardsConfiguration.HighBracketRewards;
            }
            else
            {
                rewards = _rainbowBattleRewardsConfiguration.GeneralBracketRewards;
            }
            
            int doubleRewardChance = 0;
            
            if (session.PlayerEntity.BCardComponent.HasBCard(BCardType.RainbowBattleEffects,
                    (byte)AdditionalTypes.RainbowBattleEffects.DoubleRewardsChanceIncreaseInRainbowBattle))
            {
                doubleRewardChance = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(
                    BCardType.RainbowBattleEffects,
                    (byte)AdditionalTypes.RainbowBattleEffects.DoubleRewardsChanceIncreaseInRainbowBattle,
                    session.PlayerEntity.Level).firstData;
            }
            else if (session.PlayerEntity.BCardComponent.HasBCard(BCardType.RainbowBattleEffects,
                         (byte)AdditionalTypes.RainbowBattleEffects.DoubleRewardsChanceDecreaseInRainbowBattle))
            {
                doubleRewardChance = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(
                    BCardType.RainbowBattleEffects,
                    (byte)AdditionalTypes.RainbowBattleEffects.DoubleRewardsChanceDecreaseInRainbowBattle,
                    session.PlayerEntity.Level).firstData;
                
                doubleRewardChance = Math.Max(0, 100 - doubleRewardChance);
            }
            
            foreach (RainbowBattleReward reward in rewards)
            {
                await AddReward(session, reward.ItemVnum, reward.Amount);
                
                if (doubleRewardChance <= 0 || _randomGenerator.RandomNumber() >= doubleRewardChance)
                {
                    continue;
                }

                await AddReward(session, reward.ItemVnum, reward.Amount);
            }
        }


        private async Task AddReward(IClientSession session, short itemVnum, int quantity)
        {
            GameItemInstance rewards = _gameItemInstanceFactory.CreateItem(itemVnum, quantity);
            await session.AddNewItemToInventory(rewards, true, sendGiftIsFull: true);
        }

        private void ProcessFamilyMission(IClientSession session)
        {
            if (!session.PlayerEntity.IsInFamily())
            {
                return;
            }

            _familyMissionManager.IncrementFamilyMission(session.PlayerEntity.Family.Id, session.PlayerEntity.Id, (int)FamilyMissionVnums.DAILY_COMPLETE_10_RAINBOW_BATTLE);
        }

        private void ProcessFamilyWinMission(IClientSession session)
        {
            if (!session.PlayerEntity.IsInFamily())
            {
                return;
            }

            _familyMissionManager.IncrementFamilyMission(session.PlayerEntity.Family.Id, session.PlayerEntity.Id, (int)FamilyMissionVnums.DAILY_WIN_5_RAINBOW_BATTLE);
        }
    }
}