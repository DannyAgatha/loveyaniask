using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsAPI.Scripting.Event.Common;
using WingsEmu.Game;
using WingsEmu.Game.Maps.Event;

namespace WingsAPI.Scripting.Converter
{
    public class SRemovePortalEventConverter : ScriptedEventConverter<SRemovePortalEvent>
    {
        private readonly Dictionary<Guid, IPortalEntity> _portals;

        public SRemovePortalEventConverter(Dictionary<Guid, IPortalEntity> portals) => _portals = portals;

        protected override IAsyncEvent Convert(SRemovePortalEvent e) => new PortalRemoveEvent(_portals[e.Portal.Id]);
    }
}