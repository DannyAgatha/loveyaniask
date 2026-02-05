using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.TimeSpace;
using WingsEmu.Game;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Scripting;

public class STogglePortalEventConverter : ScriptedEventConverter<STogglePortalEvent>
{
    private readonly Dictionary<Guid, IPortalEntity> _portals;

    public STogglePortalEventConverter(Dictionary<Guid, IPortalEntity> portals) => _portals = portals;

    protected override IAsyncEvent Convert(STogglePortalEvent e) =>
        new TimeSpaceTogglePortalEvent
        {
            PortalEntity = _portals[e.Portal.Id]
        };
}