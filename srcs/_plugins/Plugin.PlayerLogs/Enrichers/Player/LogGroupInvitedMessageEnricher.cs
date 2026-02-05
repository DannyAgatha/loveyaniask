using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Player;
using WingsEmu.Game.Groups.Events;

namespace Plugin.PlayerLogs.Enrichers.Player
{
    public class LogGroupInvitedMessageEnricher : ILogMessageEnricher<GroupInvitedEvent, LogGroupInvitedMessage>
    {
        public void Enrich(LogGroupInvitedMessage message, GroupInvitedEvent e)
        {
            message.GroupId = e.Sender.PlayerEntity.GetGroupId();
            message.TargetId = e.TargetId;
        }
    }
}