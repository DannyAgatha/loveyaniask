using WingsEmu.Game._packetHandling;

namespace Plugin.PlayerLogs.Core
{
    public interface IPlayerEventLogMessageFactory<in TEvent, out TLogMessage>
    where TEvent : PlayerEvent
    where TLogMessage : IPlayerActionLogMessage
    {
        TLogMessage CreateMessage(TEvent e);
    }
}