using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Logs;

namespace Plugin.PlayerLogs.Core
{
    public class GenericPlayerGameEventToLogProcessor<TEvent, TLogMessage> : IAsyncEventProcessor<TEvent>
    where TEvent : PlayerEvent
    where TLogMessage : IPlayerActionLogMessage
    {
        private readonly IPlayerEventLogMessageFactory<TEvent, TLogMessage> _factory;
        private readonly IPlayerLogManager _playerLogManager;

        public GenericPlayerGameEventToLogProcessor(IPlayerLogManager playerLogManager, IPlayerEventLogMessageFactory<TEvent, TLogMessage> factory)
        {
            _playerLogManager = playerLogManager;
            _factory = factory;
        }

        public Task HandleAsync(TEvent e, CancellationToken cancellation)
        {
            _playerLogManager.AddLog(_factory.CreateMessage(e));


            return Task.CompletedTask;
        }
    }
}