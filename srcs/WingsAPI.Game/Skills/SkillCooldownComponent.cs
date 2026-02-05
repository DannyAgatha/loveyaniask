using System;
using System.Collections.Concurrent;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Skills;

public class SkillCooldownComponent : ISkillCooldownComponent
{
    public SkillCooldownComponent()
    {
        SkillCooldowns = new ConcurrentQueue<(DateTime time, short castId)>();
        MatesSkillCooldowns = new ConcurrentQueue<(DateTime time, short castId, MateType mateType)>();
    }

    public ConcurrentQueue<(DateTime time, short castId)> SkillCooldowns { get; set; }
    public ConcurrentQueue<(DateTime time, short castId, MateType mateType)> MatesSkillCooldowns { get; }

    public void AddSkillCooldown(DateTime time, short castId)
    {
        SkillCooldowns.Enqueue((time, castId));
    }

    public void ClearSkillCooldowns()
    {
        SkillCooldowns.Clear();
    }
    
    public void ClearSkillCooldownsById(short id)
    {
        ConcurrentQueue<(DateTime time, short castId)> newSkillCooldowns = new();

        while (SkillCooldowns.TryDequeue(out (DateTime time, short castId) item))
        {
            if (item.castId != id)
            {
                newSkillCooldowns.Enqueue(item);
            }
        }

        SkillCooldowns = newSkillCooldowns;
    }

    public void AddMateSkillCooldown(DateTime time, short castId, MateType mateType)
    {
        MatesSkillCooldowns.Enqueue((time, castId, mateType));
    }

    public void ClearMateSkillCooldowns()
    {
        MatesSkillCooldowns.Clear();
    }
}