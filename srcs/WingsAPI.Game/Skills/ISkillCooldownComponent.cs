using System;
using System.Collections.Concurrent;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Skills;

public interface ISkillCooldownComponent
{
    public ConcurrentQueue<(DateTime time, short castId)> SkillCooldowns { get; }
    public ConcurrentQueue<(DateTime time, short castId, MateType mateType)> MatesSkillCooldowns { get; }

    public void AddSkillCooldown(DateTime time, short castId);
    public void ClearSkillCooldownsById(short id);
    public void ClearSkillCooldowns();

    public void AddMateSkillCooldown(DateTime time, short castId, MateType mateType);
    public void ClearMateSkillCooldowns();
}

public interface IEntitySkillFactory
{
    INpcMonsterSkill CreateNpcMonsterSkill(int skillVnum, short rate, bool isBasicAttack, bool isIgnoringHitChance);
}