using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Data.BattlePass;
using WingsAPI.Packets.ClientPackets;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.BattlePass;

public class GetBattlePassQuestRewardPacketHandler : GenericGamePacketHandlerBase<BpMSelPacket>
{
    private readonly BattlePassQuestConfiguration _battlePassQuestConfiguration;

    public GetBattlePassQuestRewardPacketHandler(BattlePassQuestConfiguration battlePassQuestConfiguration)
    {
        _battlePassQuestConfiguration = battlePassQuestConfiguration;
    }

    protected override async Task HandlePacketAsync(IClientSession session, BpMSelPacket packet)
    {
        BattlePassQuest quest = _battlePassQuestConfiguration.Quests.FirstOrDefault(s => s.Id == packet.QuestId);

        if (quest == null)
        {
            return;
        }

        BattlePassQuestDto questLog = session.PlayerEntity.BattlePassQuestDto.FirstOrDefault(s => s.QuestId == packet.QuestId);

        if (questLog == null)
        {
            return;
        }

        if (questLog.RewardAlreadyTaken)
        {
            return;
        }

        if (quest.MaxObjectiveValue != questLog.Advancement)
        {
            return;
        }

        if (quest.RewardsType == RewardsType.Points)
        {
            session.PlayerEntity.BattlePassOptionDto.Points += quest.RewardAmount;
            session.SendMsg($"You got {quest.RewardAmount} BattlePass Points !", MsgMessageType.MiddleYellow);
        }

        if (quest.RewardsType == RewardsType.Jewels)
        {
            session.PlayerEntity.BattlePassOptionDto.Jewels += quest.RewardAmount;
            session.SendMsg($"You got {quest.RewardAmount} BattlePass Jewels !", MsgMessageType.MiddleYellow);
        }

        questLog.RewardAlreadyTaken = true;
        questLog.AccomplishedDate = DateTime.Now;
        await session.EmitEventAsync(new BattlePassQuestPacketEvent());
        await session.EmitEventAsync(new BattlePassItemPacketEvent());
    }
}