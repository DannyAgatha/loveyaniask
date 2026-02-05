using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.Common;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Scripting.Converter;

public class STeleportMembersEventConverter : ScriptedEventConverter<STeleportMembersEvent>
{
    private readonly Dictionary<Guid, RaidSubInstance> _raidSubInstances;

    public STeleportMembersEventConverter(Dictionary<Guid, RaidSubInstance> raidSubInstances) => _raidSubInstances = raidSubInstances;

    protected override IAsyncEvent Convert(STeleportMembersEvent e)
    {
        IMapInstance mapInstance = _raidSubInstances[e.MapInstanceId].MapInstance;
        var sourcePosition = new Position(e.SourcePosition.X, e.SourcePosition.Y);
        var destinationPosition = new Position(e.DestinationPosition.X, e.DestinationPosition.Y);
        return new RaidTeleportMemberEvent(mapInstance, sourcePosition, destinationPosition, e.Range);
    }
}