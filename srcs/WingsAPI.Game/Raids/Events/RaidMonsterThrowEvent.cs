using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Core;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Raids.Events;

public class RaidMonsterThrowEvent : IAsyncEvent
{
    public RaidMonsterThrowEvent(IMonsterEntity monsterEntity, List<Drop> drops, byte itemDropsAmount, Range<int> goldDropRange, byte goldDropsAmount)
    {
        MonsterEntity = monsterEntity;
        Drops = drops;
        ItemDropsAmount = itemDropsAmount;
        GoldDropRange = goldDropRange;
        GoldDropsAmount = goldDropsAmount;
    }

    public IMonsterEntity MonsterEntity { get; }

    public List<Drop> Drops { get; }

    public byte ItemDropsAmount { get; }

    public Range<int> GoldDropRange { get; }

    public byte GoldDropsAmount { get; }
}

public class Drop
{
    public int ItemVNum { get; set; }

    public int Amount { get; set; }
}