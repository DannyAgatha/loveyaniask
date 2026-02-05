using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages;
using WingsEmu.Game;

namespace Plugin.PlayerLogs.Enrichers
{
    public class LogGmCommandExecutedMessageEnricher : ILogMessageEnricher<GmCommandEvent, LogGmCommandExecutedMessage>
    {
        public void Enrich(LogGmCommandExecutedMessage message, GmCommandEvent e)
        {
            message.PlayerAuthority = e.PlayerAuthority;
            message.Command = e.Command;
            message.CommandAuthority = e.CommandAuthority;
        }
    }
}