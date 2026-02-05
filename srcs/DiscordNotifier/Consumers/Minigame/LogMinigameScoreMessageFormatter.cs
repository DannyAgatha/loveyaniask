using DiscordNotifier.Formatting;
using Plugin.PlayerLogs.Messages.Miniland;

namespace DiscordNotifier.Consumers.Minigame
{
    public class LogMinigameScoreMessageFormatter : IDiscordLogFormatter<LogMinigameScoreMessage>
    {
        public LogType LogType => LogType.MINIGAME_SCORE;

        public bool TryFormat(LogMinigameScoreMessage message, out string formattedString)
        {
            formattedString =
                $"{message.CreatedAt:yyyy-MM-dd HH:mm:ss} | CHANNEL {message.ChannelId} | {message.CharacterName} | MINIGAME {message.MinigameType} | OWNER_ID {message.OwnerId} | SCORE1 {message.Score1} |"
                + $"SCORE2 {message.Score2} | VALIDITY {message.ScoreValidity} | COMPLETION_TIME {message.CompletionTime.ToString()}";
            return true;
        }
    }
}