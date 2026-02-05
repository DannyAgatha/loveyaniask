using System;
using System.Collections.Generic;

namespace WingsEmu.Game.Mates.PartnerFusion;

public static class PartnerFusionExtensions
{
    public static List<(Func<int, bool> Check, string Message)> GetStatInfo()
    {
        return new List<(Func<int, bool> Check, string Message)>
        {
            (x => x > 0, "Attacks are increased by {0} (+{1})\n"),
            (x => x > 0, "Defence is increased by {0} (+{1})\n"),
            (x => x > 0, "Damage reduction from critical hits is increased by {0} (+{1})\n"),
            (x => x > 0, "HP/MP are increased by {0} (+{1})\n"),
            (x => x > 0, "Fire resistance is increased by {0} (+{1})\n"),
            (x => x > 0, "Water resistance is increased by {0} (+{1})\n"),
            (x => x > 0, "Light resistance is increased by {0} (+{1})\n"),
            (x => x > 0, "Shadow resistance is increased by {0} (+{1})")
        };
    }

    public static int GetStatValue(this PartnerFusionResult result, int index)
    {
        return index switch
        {
            0 => result.Damage,
            1 => result.Defence,
            2 => result.CriticalDefence,
            3 => result.HpMp,
            4 => result.FireRes,
            5 => result.WaterRes,
            6 => result.LightRes,
            7 => result.ShadowRes,
            _ => 0
        };
    }

    public static int GetMultiplier(int index)
    {
        return index switch
        {
            0 => 20,
            1 => 10,
            2 => 1,
            3 => 300,
            _ => 1
        };
    }
}