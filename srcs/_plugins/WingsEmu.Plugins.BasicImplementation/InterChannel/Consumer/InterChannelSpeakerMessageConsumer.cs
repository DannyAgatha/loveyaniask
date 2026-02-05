using PhoenixLib.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer
{
    public class InterChannelSpeakerMessageConsumer : IMessageConsumer<InterChannelSpeakerMessage>
    {
        private readonly ISessionManager _sessionManager;

        public InterChannelSpeakerMessageConsumer(ISessionManager serverManager)
        {
            _sessionManager = serverManager;
        }

        public async Task HandleAsync(InterChannelSpeakerMessage notification, CancellationToken token)
        {
            if (notification.ChannelId == StaticServerManager.Instance.ChannelId)
            {
                return;
            }

            _sessionManager.Broadcast(s => notification.Message, new SpeakerHeroBroadcast(), new ExpectBlockedPlayerBroadcast(notification.UserId));
        }  
    }
}