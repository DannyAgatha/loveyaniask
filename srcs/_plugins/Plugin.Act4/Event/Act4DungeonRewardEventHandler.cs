using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Plugin.Act4.Const;
using Plugin.Act4.Extension;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Entities;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.SubClass;
using WingsEmu.Game.Families.Enum;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.Act4.Event;

public class Act4DungeonRewardEventHandler : IAsyncEventProcessor<Act4DungeonRewardEvent>
{
    private readonly Act4DungeonsConfiguration _act4DungeonsConfiguration;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IGameItemInstanceFactory _gameItemInstance;
    private readonly IGameLanguageService _languageService;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IEvtbConfiguration _evtbConfiguration;

    public Act4DungeonRewardEventHandler(IAsyncEventPipeline asyncEventPipeline, IGameItemInstanceFactory gameItemInstance, IRandomGenerator randomGenerator,
        Act4DungeonsConfiguration act4DungeonsConfiguration, IGameLanguageService languageService, IEvtbConfiguration evtbConfiguration)
    {
        _asyncEventPipeline = asyncEventPipeline;
        _gameItemInstance = gameItemInstance;
        _randomGenerator = randomGenerator;
        _act4DungeonsConfiguration = act4DungeonsConfiguration;
        _languageService = languageService;
        _evtbConfiguration = evtbConfiguration;
    }

    public async Task HandleAsync(Act4DungeonRewardEvent e, CancellationToken cancellation)
    {
        // TODO: Add Raid Mode Type for Act4 Raids - Dazynnn
        DungeonInstance dungeonInstance = e.DungeonInstanceWrapper.DungeonInstance;
        DungeonSubInstance bossMap = dungeonInstance.DungeonSubInstances.Values.FirstOrDefault(x => 0 < x.Bosses.Count);
        if (bossMap == null)
        {
            Log.Warn($"[ACT4_DUNGEON_SYSTEM] Can't give the Dungeon's Reward due to the impossibility of finding the bossMap. DungeonType: '{dungeonInstance.DungeonType.ToString()}'");
            await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonStopEvent
            {
                DungeonInstance = dungeonInstance
            }, cancellation);
            return;
        }

        RaidReward raidReward = dungeonInstance.DungeonReward;

        dungeonInstance.FinishSlowMoDate = DateTime.UtcNow + _act4DungeonsConfiguration.DungeonSlowMoDelay;

        dungeonInstance.CleanUpBossMapDate = dungeonInstance.FinishSlowMoDate + _act4DungeonsConfiguration.DungeonBossMapClosureAfterReward;
        bossMap.AddEvent(DungeonConstEventKeys.RaidSubInstanceCleanUp, new Act4DungeonBossMapCleanUpEvent
        {
            DungeonInstance = dungeonInstance,
            BossMap = bossMap
        });

        bossMap.LoopWaves.Clear();
        bossMap.LinearWaves.Clear();

        if (dungeonInstance.DungeonType == DungeonType.Hatus)
        {
            bossMap.HatusHeads.HeadsState = HatusDragonHeadState.HIDE_HEAD;
            bossMap.MapInstance.Broadcast(Act4DungeonExtension.HatusHeadStatePacket(7, bossMap.HatusHeads));
        }

        bool createRaidFinishLog = true;

        var members = bossMap.MapInstance.Sessions.ToList();

        var randomBag = new RandomBag<RaidBoxRarity>(_randomGenerator);
        foreach (RaidBoxRarity toAdd in raidReward.RaidBox.RaidBoxRarities)
        {
            randomBag.AddEntry(toAdd, toAdd.Chance);
        }
        
        int randomNumber = _randomGenerator.RandomNumber();

        foreach (IClientSession member in members.Where(member => member != null))
        {
            if (createRaidFinishLog)
            {
                createRaidFinishLog = false;
                await member.FamilyAddLogAsync(FamilyLogType.RaidWon, ((short)dungeonInstance.DungeonType).ToString());
                await member.FamilyAddExperience(10000 / bossMap.MapInstance.Sessions.Count, FamXpObtainedFromType.Raid);
            }

            var rewardBoxMap = new List<(DungeonType, int, int)>
            {
                (DungeonType.Morcos, 25346, 25347),
                (DungeonType.Hatus, 25344, 25345),
                (DungeonType.Calvinas, 25340, 25341),
                (DungeonType.Berios, 25342, 25343)
            };

            if (rewardBoxMap.Any(box => box.Item1 == dungeonInstance.DungeonType))
            {
                (DungeonType, int, int) rewardBox = rewardBoxMap.First(box => box.Item1 == dungeonInstance.DungeonType);

                if (randomNumber <= 25)
                {
                    int rewardBoxId = randomNumber <= 12 ? rewardBox.Item2 : rewardBox.Item3;
                    GameItemInstance rewardBoxItem = _gameItemInstance.CreateItem(rewardBoxId, 1, 0, (sbyte)(randomNumber <= 12 ? 7 : 8));
                    await member.AddNewItemToInventory(rewardBoxItem, true, ChatMessageColorType.Yellow, true);

                    if (randomNumber < _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_CHANCE_GET_SECOND_RAIDBOX))
                    {
                        member.SendEffect(EffectType.DoubleChanceDrop);
                        GameItemInstance rewardBoxItem2 = _gameItemInstance.CreateItem(rewardBoxId, 1, 0, (sbyte)(randomNumber <= 12 ? 7 : 8));
                        await member.AddNewItemToInventory(rewardBoxItem2, true, ChatMessageColorType.Yellow, true);
                    }
                }
                else
                {
                    GameItemInstance rewardBoxSimple = _gameItemInstance.CreateItem(raidReward.RaidBox.RewardBox, 1, 0);
                    await member.AddNewItemToInventory(rewardBoxSimple, true, ChatMessageColorType.Yellow, true);

                    if (randomNumber < _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_CHANCE_GET_SECOND_RAIDBOX))
                    {
                        member.SendEffect(EffectType.DoubleChanceDrop);
                        GameItemInstance rewardBox2 = _gameItemInstance.CreateItem(raidReward.RaidBox.RewardBox, 1, 0);
                        await member.AddNewItemToInventory(rewardBox2, true, ChatMessageColorType.Yellow, true);
                    }
                }
            }

            int baseExperience = member.PlayerEntity.SubClass.IsPvpSubClass() ? 2000 : member.PlayerEntity.SubClass.IsPvpAndPveSubClass() ? 1000 : 0;
            member.AddTierExperience(baseExperience, _languageService, false);
            member.SendMsg(_languageService.GetLanguage(GameDialogKey.ACT4_DUNGEON_SHOUTMESSAGE_BOSS_COMPLETED, member.UserLanguage), MsgMessageType.Middle);
            member.SendEmptyRaidBoss();
        }


        await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonWonEvent
        {
            DungeonInstance = dungeonInstance,
            DungeonLeader = members[0],
            Members = members
        }, cancellation);

        await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonBroadcastPacketEvent
        {
            DungeonInstance = dungeonInstance
        }, cancellation);
    }
}