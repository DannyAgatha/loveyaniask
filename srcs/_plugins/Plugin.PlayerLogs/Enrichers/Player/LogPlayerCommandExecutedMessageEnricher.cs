using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Player;
using WingsEmu.Game;

namespace Plugin.PlayerLogs.Enrichers.Player
{
    public sealed class LogPlayerCommandExecutedMessageEnricher : ILogMessageEnricher<PlayerCommandEvent, LogPlayerCommandExecutedMessage>
    {
        public void Enrich(LogPlayerCommandExecutedMessage message, PlayerCommandEvent e)
        {
            message.Command = e.Command;
        }
    }
}