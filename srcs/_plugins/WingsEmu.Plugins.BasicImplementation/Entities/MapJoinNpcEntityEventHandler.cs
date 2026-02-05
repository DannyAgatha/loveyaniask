using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Helpers.Damages;

namespace NosEmu.Plugins.BasicImplementations.Entities;

public class MapJoinNpcEntityEventHandler : IAsyncEventProcessor<MapJoinNpcEntityEvent>
{
    private readonly IRandomGenerator _randomGenerator;

    public MapJoinNpcEntityEventHandler(IRandomGenerator randomGenerator) => _randomGenerator = randomGenerator;

    public async Task HandleAsync(MapJoinNpcEntityEvent e, CancellationToken cancellation)
    {
        INpcEntity npcEntity = e.NpcEntity;
        if (npcEntity.MapInstance != null)
        {
            // await npcEntity.EmitEventAsync(new MapLeaveNpcEntityEvent(npcEntity));
        }

        short x = e.MapX ?? npcEntity.PositionX;
        short y = e.MapY ?? npcEntity.PositionY;
        npcEntity.ChangePosition(new Position(x, y));
        npcEntity.FirstX = x;
        npcEntity.FirstY = y;
        npcEntity.NextTick = DateTime.UtcNow;
        npcEntity.NextTick += TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(1000));
        npcEntity.NextAttackReady = DateTime.UtcNow;

        npcEntity.MapInstance?.AddNpc(npcEntity);
        string inPacket = npcEntity.GenerateIn();
        npcEntity.MapInstance?.Broadcast(inPacket);
    }
}