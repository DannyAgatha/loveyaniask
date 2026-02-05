// NosEmu
// 


using System;
using System.Collections.Generic;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Buffs;

public class Buff
{
    public Buff(Guid buffId, int cardId, int level, TimeSpan duration, int effectId, string name,
        short timeoutBuff, BuffGroup buffGroup, byte timeoutBuffChance, int secondBCardsDelay,
        int groupId, BuffCategory buffCategory, DateTime start, BuffFlag buffFlags, IReadOnlyCollection<BCardDTO> bCards,
        bool isConstEffect, ElementType elementType, int casterLevel, IBattleEntity caster, bool isFirstApply)
    {
        BuffId = buffId;
        CardId = cardId;
        Level = level;
        Duration = duration;
        EffectId = effectId;
        GroupId = groupId;
        Name = name;
        TimeoutBuff = timeoutBuff;
        BuffGroup = buffGroup;
        TimeoutBuffChance = timeoutBuffChance;
        SecondBCardsDelay = secondBCardsDelay;
        BuffCategory = buffCategory;
        Start = start;
        BuffFlags = buffFlags;
        BCards = bCards;
        IsConstEffect = isConstEffect;
        ElementType = elementType;
        CasterLevel = casterLevel;
        Caster = caster;
        IsFirstApply = isFirstApply;
    }
    
    public bool IsFirstApply { get;  set;}

    public Guid BuffId { get; }

    public int CardId { get; }

    public int GroupId { get; }

    public int Level { get; set; }

    public TimeSpan Duration { get; set; }

    public int EffectId { get; }

    public string Name { get; }

    public short TimeoutBuff { get; }

    public BuffGroup BuffGroup { get; }

    public byte TimeoutBuffChance { get; }

    public int SecondBCardsDelay { get; }

    public bool SecondBCardsExecuted { get; set; }

    public BuffFlag BuffFlags { get; }

    public BuffCategory BuffCategory { get; }

    public DateTime Start { get; private set; }

    public bool IsConstEffect { get; }

    public ElementType ElementType { get; }

    public int CasterLevel { get; }

    public IBattleEntity Caster { get; }

    public IReadOnlyCollection<BCardDTO> BCards { get; }


    public void SetBuffDuration(TimeSpan duration)
    {
        Start = DateTime.UtcNow;
        Duration = duration;
    }

    protected bool Equals(Buff other) => BuffId.Equals(other.BuffId);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Buff)obj);
    }

    public override int GetHashCode() => BuffId.GetHashCode();
}