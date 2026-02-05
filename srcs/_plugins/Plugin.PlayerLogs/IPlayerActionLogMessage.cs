using PhoenixLib.ServiceBus;
using WingsEmu.Game.Logs;

namespace Plugin.PlayerLogs
{
    public interface IPlayerActionLogMessage : IMessage, IPlayerActionLog
    {
    }
}