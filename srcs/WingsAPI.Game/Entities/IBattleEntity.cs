// NosEmu
// 


using System;
using System.Collections.Generic;
using WingsEmu.Core.Generics;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.BCard;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Entities;

public interface IBattleEntity : IMoveableEntity, IEventTriggerContainer, IBattleEntityEventEmitter, ICastingComponent, IEndBuffDamageComponent
{
    public byte Level { get; set; }
    public byte HeroLevel { get; set; }
    public byte Direction { get; set; }

    byte HpPercentage => Math.Max((byte)1, (byte)(MaxHp <= 1 ? 1 : (byte)(Hp / (float)MaxHp * 100)));
    public int Hp { get; set; }
    public int MaxHp { get; set; }

    byte MpPercentage => Math.Max((byte)1, (byte)(MaxMp <= 1 ? 1 : (byte)(Mp / (float)MaxMp * 100)));
    public int Mp { get; set; }
    public int MaxMp { get; set; }

    public byte Element { get; set; }

    public int ElementRate { get; set; }

    public int FireResistance { get; set; }

    public int WaterResistance { get; set; }

    public int LightResistance { get; set; }

    public int DarkResistance { get; set; }

    public int DamagesMinimum { get; set; }
    public int DamagesMaximum { get; set; }
    public byte HitsReceived { get; set; }
    public sbyte NoDamageChance { get; set; }
    public FactionType Faction { get; }
    public IBattleEntity Killer { get; set; }
    public int Size { get; set; }
    public List<IBattleEntitySkill> Skills { get; }
    public IBuffComponent BuffComponent { get; }
    public IBCardComponent BCardComponent { get; }
    public IChargeComponent ChargeComponent { get; }
    public IBCardStackComponent BCardStackComponent { get; }
    public ThreadSafeHashSet<Guid> AggroedEntities { get; }
    public IBCardDataComponent BCardDataComponent { get; }

    public IAlzanorComponent AlzanorComponent { get; }
}