using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Npcs.Event;

public class NpcSummonEvent : IAsyncEvent
{
    public IMapInstance Map { get; init; }
    public IEnumerable<ToSummon> Npcs { get; init; }
    public Guid? MonsterId { get; set; }
}