// NosEmu
// 


using System;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Raids;

namespace WingsEmu.Game.Maps.Event;

public class JoinMapEvent : PlayerEvent
{
    public JoinMapEvent(int joinedMap, short? x = null, short? y = null, bool isMarathonMode = false, RaidParty lastRaid = null)
    {
        JoinedMapId = joinedMap;
        X = x;
        Y = y;
        IsMarathonMode = isMarathonMode;
        LastRaid = lastRaid;
    }

    public JoinMapEvent(Guid joinedMap, short? x = null, short? y = null, bool isMarathonMode = false, RaidParty lastRaid = null)
    {
        JoinedMapGuid = joinedMap;
        X = x;
        Y = y;
        IsMarathonMode = isMarathonMode;
        LastRaid = lastRaid;
    }

    public JoinMapEvent(IMapInstance joinedMap, short? x = null, short? y = null, bool isMarathonMode = false, RaidParty lastRaid = null)
    {
        JoinedMapInstance = joinedMap;
        X = x;
        Y = y;
        IsMarathonMode = isMarathonMode;
        LastRaid = lastRaid;
    }


    public int JoinedMapId { get; }

    public Guid JoinedMapGuid { get; }

    public IMapInstance JoinedMapInstance { get; }

    public short? X { get; }

    public short? Y { get; }
    
    public bool IsMarathonMode { get; }
    
    public RaidParty LastRaid { get; }
}