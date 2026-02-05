using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateReviveEvent : PlayerEvent
{
    public MateReviveEvent(IMateEntity mateEntity, bool delayed)
    {
        MateEntity = mateEntity;
        Delayed = delayed;
    }

    public MateReviveEvent(IMateEntity mateEntity, bool delayed, Guid expectedGuid)
    {
        MateEntity = mateEntity;
        Delayed = delayed;
        ExpectedGuid = expectedGuid;
    }

    public IMateEntity MateEntity { get; set; }

    public bool Delayed { get; set; }

    public Guid ExpectedGuid { get; set; }
}