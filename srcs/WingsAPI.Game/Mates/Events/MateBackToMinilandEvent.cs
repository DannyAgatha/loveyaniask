using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateBackToMinilandEvent : PlayerEvent
{
    public MateBackToMinilandEvent(IMateEntity mateEntity, Guid expectedGuid)
    {
        MateEntity = mateEntity;
        ExpectedGuid = expectedGuid;
    }

    public IMateEntity MateEntity { get; set; }

    public Guid ExpectedGuid { get; set; }
}