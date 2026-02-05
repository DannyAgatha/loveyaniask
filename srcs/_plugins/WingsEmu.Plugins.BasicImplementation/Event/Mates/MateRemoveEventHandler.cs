using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateRemoveEventHandler : IAsyncEventProcessor<MateRemoveEvent>
{
    private readonly IMateTransportFactory _mateTransportFactory;

    public MateRemoveEventHandler(IMateTransportFactory mateTransportFactory) => _mateTransportFactory = mateTransportFactory;

    public async Task HandleAsync(MateRemoveEvent e, CancellationToken cancellation)
    {
        IMateEntity mateEntity = e.MateEntity;
        IClientSession session = e.Sender;

        session.PlayerEntity.MateComponent.RemoveMate(mateEntity);

        session.SendPClearPacket();
        session.SendScpPackets();
        session.SendScnPackets();
        session.Broadcast(mateEntity.GenerateOut());
        session.CurrentMapInstance.RemoveMate(mateEntity);
    }
}