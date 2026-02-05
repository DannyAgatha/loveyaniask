using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.TimeSpace;
using WingsEmu.Game;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Scripting;

public class SClosePortalEventConverter : ScriptedEventConverter<SClosePortal>
{
    private readonly Dictionary<Guid, IPortalEntity> _portals;

    public SClosePortalEventConverter(Dictionary<Guid, IPortalEntity> portals) => _portals = portals;

    protected override IAsyncEvent Convert(SClosePortal e) =>
        new TimeSpaceClosePortalEvent
        {
            PortalEntity = _portals[e.Portal.Id]
        };
}