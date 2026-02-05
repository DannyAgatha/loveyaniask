using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;

public class InterChannelSendWhisperMessageConsumer : IMessageConsumer<InterChannelSendWhisperMessage>
{
    private readonly ISessionManager _sessionManager;

    public InterChannelSendWhisperMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

    public async Task HandleAsync(InterChannelSendWhisperMessage e, CancellationToken cancellation)
    {
        IClientSession session = _sessionManager.GetSessionByCharacterName(e.ReceiverNickname);

        if (session == null)
        {
            return;
        }

        await session.EmitEventAsync(new InterChannelReceiveWhisperEvent(e.SenderCharacterId, e.SenderNickname, e.SenderChannelId, e.AuthorityType, e.Message));
    }
}