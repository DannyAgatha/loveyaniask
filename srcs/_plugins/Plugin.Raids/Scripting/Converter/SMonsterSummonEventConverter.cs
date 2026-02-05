using System;
using System.Collections.Concurrent;
using System.Linq;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.Common;
using WingsAPI.Scripting.Object.Raid;
using WingsEmu.Game;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster;
using WingsEmu.Game.Monster.Event;

namespace Plugin.Raids.Scripting.Converter;

public class SMonsterSummonEventConverter : ScriptedEventConverter<SMonsterSummonEvent>
{
    private readonly IMapInstance _mapInstance;

    public SMonsterSummonEventConverter(IMapInstance mapInstance) => _mapInstance = mapInstance;

    protected override IAsyncEvent Convert(SMonsterSummonEvent e)
    {
        return new MonsterSummonEvent(_mapInstance, e.Monsters.Select(x =>
        {
            ConcurrentDictionary<byte, Waypoint> waypointsDictionary = null;
            if (x.Waypoints != null)
            {
                byte waypoints = 0;
                foreach (SWaypoint waypoint in x.Waypoints)
                {
                    waypointsDictionary ??= new ConcurrentDictionary<byte, Waypoint>();

                    var newWaypoint = new Waypoint
                    {
                        X = waypoint.X,
                        Y = waypoint.Y,
                        WaitTime = waypoint.WaitTime
                    };

                    waypointsDictionary.TryAdd(waypoints, newWaypoint);
                    waypoints++;
                }
            }

            return new ToSummon
            {
                VNum = x.Vnum,
                SpawnCell = x.IsRandomPosition ? null : new Position(x.Position.X, x.Position.Y),
                IsMoving = x.CanMove,
                IsTarget = x.IsTarget,
                IsBossOrMate = x.IsBoss,
                IsHostile = true,
                AtAroundMobId = string.IsNullOrEmpty(x.AtAroundMobId) ? null : Guid.Parse(x.AtAroundMobId),
                AtAroundMobRange = x.AtAroundMobRange,
                Waypoints = waypointsDictionary,
                Direction = x.Direction
            };
        }));
    }
}