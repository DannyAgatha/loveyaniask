using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalEventIceBreakerHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly RevivalEventNormalHandler _eventNormalHandler;

    public RevivalEventIceBreakerHandler(RevivalEventNormalHandler eventNormalHandler) => _eventNormalHandler = eventNormalHandler;

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        IPlayerEntity character = e.Sender.PlayerEntity;
        if (e.Sender?.CurrentMapInstance == null)
        {
            return;
        }

        if (character.IsAlive() || e.Sender.PlayerEntity.MapInstance.MapInstanceType != MapInstanceType.Icebreaker)
        {
            return;
        }

        e.Sender.SendBuffsPacket();
        // get group
        e.Sender.EmitEvent(new LeaveGroupEvent());
        await _eventNormalHandler.BaseRevive(e);
    }
}