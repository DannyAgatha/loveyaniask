using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.Upgrade
{
    public class LogShellIdentifiedMessageEnricher : ILogMessageEnricher<ShellIdentifiedEvent, LogShellIdentifiedMessage>
    {
        public void Enrich(LogShellIdentifiedMessage message, ShellIdentifiedEvent e)
        {
            message.Shell = e.Shell;
        }
    }
}