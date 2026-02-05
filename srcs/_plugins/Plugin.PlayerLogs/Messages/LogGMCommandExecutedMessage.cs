using PhoenixLib.ServiceBus.Routing;
using Plugin.PlayerLogs.Messages.Player;
using WingsEmu.DTOs.Account;

namespace Plugin.PlayerLogs.Messages
{
    [MessageType("logs.gm.commands")]
    public class LogGmCommandExecutedMessage : LogPlayerCommandExecutedMessage
    {
        public AuthorityType PlayerAuthority { get; set; }
        public AuthorityType CommandAuthority { get; set; }
    }
}