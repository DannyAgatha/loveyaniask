using System;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game.Configurations.Miniland;

namespace Plugin.PlayerLogs.Messages.Miniland
{
    [MessageType("logs.minigame.rewardclaimed")]
    public class LogMinigameRewardClaimedMessage : IPlayerActionLogMessage
    {
        public long OwnerId { get; set; }
        public int MinigameVnum { get; set; }
        public string MinigameType { get; set; }
        public RewardLevel RewardLevel { get; set; }
        public bool Coupon { get; set; }
        public int ItemVnum { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}