using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Miniland;
using WingsEmu.Game.Configurations.Miniland;

namespace DiscordNotifier.Consumers.Minigame
{
    public class LogMinigameRewardClaimedMessageFormatter : IDiscordLogFormatter<LogMinigameRewardClaimedMessage>
    {
        public LogType LogType => LogType.MINIGAME_REWARD_CLAIMED;

        public bool TryFormat(LogMinigameRewardClaimedMessage log, out string formattedString)
        {
            // For now we only want to log on DC the Lv.4 and Lv.5 rewards
            if (log.RewardLevel != RewardLevel.FourthReward && log.RewardLevel != RewardLevel.FifthReward)
            {
                formattedString = string.Empty;
                return false;
            }

            formattedString =
                $"{log.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {log.ChannelId} | {log.CharacterName} | MINIGAME {log.MinigameType} | OWNER_ID {log.OwnerId} | Lv.{(short)log.RewardLevel + 1} | ITEM {log.ItemVnum} x{log.Amount}";
            return true;
        }
    }
}