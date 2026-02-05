using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Miniland;
using WingsEmu.Game.Miniland.Events;

namespace Plugin.PlayerLogs.Enrichers.Miniland
{
    public class LogMinigameScoreMessageEnricher : ILogMessageEnricher<MinigameScoreLogEvent, LogMinigameScoreMessage>
    {
        public void Enrich(LogMinigameScoreMessage message, MinigameScoreLogEvent e)
        {
            message.OwnerId = e.OwnerId;
            message.CompletionTime = e.CompletionTime;
            message.MinigameVnum = e.MinigameVnum;
            message.MinigameType = e.MinigameType.ToString();
            message.Score1 = e.Score1;
            message.Score2 = e.Score2;
        }
    }
}