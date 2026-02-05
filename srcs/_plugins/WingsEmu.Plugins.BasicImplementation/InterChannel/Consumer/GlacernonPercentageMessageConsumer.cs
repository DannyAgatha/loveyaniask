using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;

public class GlacernonPercentageMessageConsumer : IMessageConsumer<GlacernonPercentageMessage>
{
    private readonly ISessionManager _serverManager;
    private readonly SerializableGameServer _gameServer;
    public GlacernonPercentageMessageConsumer(ISessionManager serverManager, SerializableGameServer gameServer)
    {
        _serverManager = serverManager;
        _gameServer = gameServer;
    }
    public Task HandleAsync(GlacernonPercentageMessage notification, CancellationToken token)
    {
        if (_gameServer.ChannelType == GameChannelType.ACT_4)
        {
            return Task.CompletedTask;
        }

        byte angelPercentage = notification.AngelPercentage;
        byte demonPercentage = notification.DemonPercentage;
        
        _serverManager.Broadcast(s =>
        {
            string message = s.GetLanguageFormat(GameDialogKey.INFORMATION_MESSAGE_GLACERNON_PERCENTAGE, angelPercentage, demonPercentage);
            return s.GenerateSayPacket(message, ChatMessageColorType.Yellow);
        });
        
        return Task.CompletedTask;
    }
}