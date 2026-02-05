using FluentValidation;
using WingsAPI.Scripting.Object.Raid;
using WingsAPI.Scripting.Validator.Common;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;

namespace Plugin.Raids.Scripting.Validator.Raid;

public class SRaidValidator : AbstractValidator<SRaid>
{
    public SRaidValidator(IMapManager mapManager, INpcMonsterManager npcManager, IItemsManager itemsManager)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Maps).NotEmpty();
        RuleFor(x => x.Requirement).SetValidator(new SRaidRequirementValidator());
        RuleFor(x => x.Spawn).SetValidator(new SLocationValidator());
        RuleFor(x => x.RaidType).IsInEnum();

        RuleForEach(x => x.Maps).SetValidator(new SMapValidator(mapManager, npcManager, itemsManager));
    }
}