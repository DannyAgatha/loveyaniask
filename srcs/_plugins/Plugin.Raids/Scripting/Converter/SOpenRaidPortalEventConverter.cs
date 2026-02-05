using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsAPI.Scripting.Converter;
using WingsAPI.Scripting.Event.Common;
using WingsEmu.Game;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Scripting.Converter;

public class SOpenRaidPortalEventConverter : ScriptedEventConverter<SOpenPortalEvent>
{
    private readonly Dictionary<Guid, IPortalEntity> portals;
    private readonly RaidSubInstance raidSubInstance;

    public SOpenRaidPortalEventConverter(RaidSubInstance raidSubInstance, Dictionary<Guid, IPortalEntity> portals)
    {
        this.raidSubInstance = raidSubInstance;
        this.portals = portals;
    }

    protected override IAsyncEvent Convert(SOpenPortalEvent e) => new RaidPortalOpenEvent(raidSubInstance, portals[e.Portal.Id]);
}