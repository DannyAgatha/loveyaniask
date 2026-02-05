using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Item;
using WingsEmu.Game.Items.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.GameEvents.Consumers
{
    public class ItemRestrictionRefreshMessageConsumer : IMessageConsumer<ItemRestrictionRefreshMessage>
    {
        private readonly ISessionManager _sessionManager;

        public ItemRestrictionRefreshMessageConsumer(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }
    
        public async Task HandleAsync(ItemRestrictionRefreshMessage notification, CancellationToken token)
        {
            IReadOnlyList<IClientSession> sessions = _sessionManager.Sessions;

            foreach (IClientSession session in sessions)
            {
                await session.EmitEventAsync(new ItemResetRestrictionEvent());
            }
        }
    }
}