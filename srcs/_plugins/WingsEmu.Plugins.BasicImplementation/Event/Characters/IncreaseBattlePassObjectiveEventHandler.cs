using PhoenixLib.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Data.BattlePass;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class IncreaseBattlePassObjectiveEventHandler : IAsyncEventProcessor<IncreaseBattlePassObjectiveEvent>
{
    private readonly BattlePassQuestConfiguration _battlePassQuestConfiguration;
    private readonly BattlePassConfiguration _battlePassConfiguration;

    public IncreaseBattlePassObjectiveEventHandler(BattlePassQuestConfiguration battlePassQuestConfiguration, BattlePassConfiguration battlePassConfiguration)
    {
        _battlePassQuestConfiguration = battlePassQuestConfiguration;
        _battlePassConfiguration = battlePassConfiguration;
    }
    
    public async Task HandleAsync(IncreaseBattlePassObjectiveEvent e, CancellationToken cancellation)
    {
        if (!_battlePassQuestConfiguration.Quests.Any(s => s.MissionType == e.MissionType && s.FirstData == e.FirstData && !s.IsQuestExpired(_battlePassConfiguration.EndSeason)))
        {
            return;
        }
        
        DateTime now = DateTime.Now;

        foreach (BattlePassQuest quest in _battlePassQuestConfiguration.Quests.Where(s => s.MissionType == e.MissionType && s.FirstData == e.FirstData && !s.IsQuestExpired(_battlePassConfiguration.EndSeason)))
        {
            BattlePassQuestDto questLog = e.Sender.PlayerEntity.BattlePassQuestDto.FirstOrDefault(s => s.QuestId == quest.Id && s.FrequencyType == quest.FrequencyType);

            if (questLog == null)
            {
                e.Sender.PlayerEntity.BattlePassQuestDto.Add(new BattlePassQuestDto()
                {
                    QuestId = quest.Id,
                    RewardAlreadyTaken = false,
                    Advancement = e.AmountToIncrease,
                    FrequencyType = quest.FrequencyType,
                    AccomplishedDate = now
                });
                
                if (!e.Sender.PlayerEntity.HaveBpUiOpen)
                {
                    continue;
                }
                
                await e.Sender.EmitEventAsync(new BattlePassQuestPacketEvent());
                continue;
            }
            
            if (quest.MissionType == MissionType.LoginXTimeInARow && 
                questLog.AccomplishedDate.Day == now.Day && 
                questLog.AccomplishedDate.Year == now.Year && 
                questLog.AccomplishedDate.Month == now.Month)
            {
                continue;
            }
            
            if (questLog.Advancement + e.AmountToIncrease > quest.MaxObjectiveValue) 
            { 
                e.AmountToIncrease = quest.MaxObjectiveValue; 
            }
            questLog.Advancement = e.AmountToIncrease >= quest.MaxObjectiveValue ? quest.MaxObjectiveValue : questLog.Advancement + e.AmountToIncrease;
            questLog.AccomplishedDate = now;
        }
        
        if (!e.Sender.PlayerEntity.HaveBpUiOpen)
        {
            return;
        }
        
        await e.Sender.EmitEventAsync(new BattlePassQuestPacketEvent());
    }
}