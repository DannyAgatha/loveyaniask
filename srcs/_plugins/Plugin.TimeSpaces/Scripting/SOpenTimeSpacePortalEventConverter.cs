using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.Common;
using WingsEmu.Game;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Scripting;

public class SOpenTimeSpacePortalEventConverter : ScriptedEventConverter<SOpenPortalEvent>
{
    private readonly Dictionary<Guid, IPortalEntity> _portals;

    public SOpenTimeSpacePortalEventConverter(Dictionary<Guid, IPortalEntity> portals) => _portals = portals;

    protected override IAsyncEvent Convert(SOpenPortalEvent e)
    {
        IPortalEntity portal = _portals[e.Portal.Id];
        return new TimeSpacePortalOpenEvent
        {
            PortalEntity = portal
        };
    }
}