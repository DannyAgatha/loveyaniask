using PhoenixLib.Events;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Raids.Events;

public class RaidProcessJumpEvent : IAsyncEvent
{
    public IMonsterEntity MonsterEntity { get; set; }
}