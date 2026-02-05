using PhoenixLib.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Data.BattlePass;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.BattlePass;

public class BattlePassQuestPacketEventHandler : IAsyncEventProcessor<BattlePassQuestPacketEvent>
{
    private readonly BattlePassQuestConfiguration _battlePassQuestConfiguration;
    private readonly BattlePassConfiguration _battlePassConfiguration;

    public BattlePassQuestPacketEventHandler(BattlePassQuestConfiguration battlePassQuestConfiguration, BattlePassConfiguration battlePassConfiguration)
    {
        _battlePassQuestConfiguration = battlePassQuestConfiguration;
        _battlePassConfiguration = battlePassConfiguration;
    }
    
    public async Task HandleAsync(BattlePassQuestPacketEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        string startSeason = _battlePassConfiguration.StartSeason.ToString("yyMMddHH");
        string endSeason = _battlePassConfiguration.EndSeason.ToString("yyMMddHH");

        StringBuilder stringBuilder = new($"bpm {_battlePassQuestConfiguration.Quests.Count} {(_battlePassConfiguration.BattlePassSeason == 0 ? 1 : _battlePassConfiguration.BattlePassSeason)} {_battlePassConfiguration.MaxBattlePassPoints} {startSeason} {endSeason}");

        foreach (BattlePassQuest quest in _battlePassQuestConfiguration.Quests.Where(s => !s.IsQuestExpired(_battlePassConfiguration.EndSeason)))
        {
            BattlePassQuestDto characterAchievement = session.PlayerEntity.BattlePassQuestDto.FirstOrDefault(s => s.QuestId == quest.Id && s.FrequencyType == quest.FrequencyType);
            long actualAchievement = characterAchievement?.Advancement ?? 0;
            if (characterAchievement != null && characterAchievement.RewardAlreadyTaken)
            {
                actualAchievement = -1;
            }
            stringBuilder.Append($" {quest.Id} {(byte)quest.MissionType} {(byte)quest.FrequencyType} {actualAchievement} {quest.MaxObjectiveValue} {quest.FirstData} {quest.RewardAmount} {(long)(Math.Round(quest.GetTotalMinutesBeforeQuestEnd(_battlePassConfiguration.EndSeason)))} {(byte)quest.RewardsType}");
        }

        session.SendPacket(stringBuilder.ToString());
    }
}