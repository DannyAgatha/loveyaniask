using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Packets.Enums;
using WingsEmu.Game;
using WingsEmu.Game.Configurations;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public interface IPartnerSpecialistSkillRoll
{
    byte GetRandomSkillRank();
}

public class PartnerSpecialistSkillRoll : IPartnerSpecialistSkillRoll
{
    private readonly RandomBag<PartnerSpecialistSkillChances> _randomRanks;

    public PartnerSpecialistSkillRoll(PartnerSpecialistSkillRollConfiguration randomRarities, IRandomGenerator randomGenerator, IEvtbConfiguration evtbConfiguration)
    {
        var skillRanks = randomRarities.OrderBy(s => s.Chance).ToList();

        _randomRanks = new RandomBag<PartnerSpecialistSkillChances>(randomGenerator);

        foreach (PartnerSpecialistSkillChances skill in skillRanks)
        {
            _randomRanks.AddEntry(skill, skill.Chance * (1 + evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_CHANCE_GET_HIGHER_PARTNER_SKILLS) * 0.01));
        }

    }

    public byte GetRandomSkillRank() => _randomRanks.GetRandom().SkillRank;
}

public class PartnerSpecialistSkillRollConfiguration : List<PartnerSpecialistSkillChances>
{
}

public class PartnerSpecialistSkillChances
{
    public int Chance { get; set; }
    public byte SkillRank { get; set; }
}