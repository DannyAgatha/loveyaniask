using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4.Event;

public class Act4SystemFcBroadcastEventHandler : IAsyncEventProcessor<Act4SystemFcBroadcastEvent>
{
    private readonly IAct4Manager _act4Manager;
    private readonly ISessionManager _sessionManager;

    public Act4SystemFcBroadcastEventHandler(IAct4Manager act4Manager, ISessionManager sessionManager)
    {
        _act4Manager = act4Manager;
        _sessionManager = sessionManager;
    }

    public async Task HandleAsync(Act4SystemFcBroadcastEvent e, CancellationToken cancellation)
    {
        Act4Status status = _act4Manager.GetStatus();
        string angelPacket = UiPacketExtension.GenerateFcPacket(FactionType.Angel, status);
        string demonPacket = UiPacketExtension.GenerateFcPacket(FactionType.Demon, status);

        _sessionManager.Broadcast(angelPacket, new FactionBroadcast(FactionType.Angel));
        _sessionManager.Broadcast(demonPacket, new FactionBroadcast(FactionType.Demon));
    }
}