// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.InstantBattle;
using WingsEmu.Game.GameEvent;
using WingsEmu.Plugins.GameEvents.Event.Global;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class InstantBattleStartMessageConsumer : IMessageConsumer<InstantBattleStartMessage>
    {
        private readonly IAsyncEventPipeline _eventPipeline;

        public InstantBattleStartMessageConsumer(IAsyncEventPipeline eventPipeline) => _eventPipeline = eventPipeline;

        public async Task HandleAsync(InstantBattleStartMessage notification, CancellationToken token)
        {
            await _eventPipeline.ProcessEventAsync(new GameEventPrepareEvent(GameEventType.InstantBattle)
            {
                NoDelay = notification.HasNoDelay
            });
        }
    }
}