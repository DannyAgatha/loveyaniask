using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Serilog;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Handlers;

public class RaidTeleportMemberEventHandler : IAsyncEventProcessor<RaidTeleportMemberEvent>
{
    private readonly IRandomGenerator _randomGenerator;

    public RaidTeleportMemberEventHandler(IRandomGenerator randomGenerator) => _randomGenerator = randomGenerator;

    public async Task HandleAsync(RaidTeleportMemberEvent e, CancellationToken cancellation)
    {
        short sourceX = e.SourcePosition.X;
        short sourceY = e.SourcePosition.Y;

        short destX = e.DestinationPosition.X;
        short destY = e.DestinationPosition.Y;
        byte range = e.Range;
        IMapInstance mapInstance = e.MapInstance;

        if (mapInstance == null)
        {
            Log.Debug("MapInstance for RaidMemberTeleport is null.");
            return;
        }

        IEnumerable<IPlayerEntity> membersToTeleport = mapInstance.GetCharactersInRange(e.SourcePosition, range);
        foreach (IPlayerEntity member in membersToTeleport)
        {
            if (member == null)
            {
                continue;
            }

            if (!member.IsAlive())
            {
                continue;
            }

            short x = (short)(destX + _randomGenerator.RandomNumber(-3, 3));
            short y = (short)(destY + _randomGenerator.RandomNumber(-3, 3));

            if (mapInstance.IsBlockedZone(x, y))
            {
                x = destX;
                y = destY;
            }

            member.TeleportOnMap(x, y, true);
        }
    }
}