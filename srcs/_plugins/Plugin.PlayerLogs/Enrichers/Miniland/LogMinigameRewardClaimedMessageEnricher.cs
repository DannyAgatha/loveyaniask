using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Miniland;
using WingsEmu.Game.Miniland.Events;

namespace Plugin.PlayerLogs.Enrichers.Miniland
{
    public class LogMinigameRewardClaimedMessageEnricher : ILogMessageEnricher<MinigameRewardClaimedEvent, LogMinigameRewardClaimedMessage>
    {
        public void Enrich(LogMinigameRewardClaimedMessage message, MinigameRewardClaimedEvent e)
        {
            message.OwnerId = e.OwnerId;
            message.MinigameVnum = e.MinigameVnum;
            message.MinigameType = e.MinigameType.ToString();
            message.RewardLevel = e.RewardLevel;
            message.Coupon = e.Coupon;
            message.ItemVnum = e.ItemVnum;
            message.Amount = e.Amount;
        }
    }
}