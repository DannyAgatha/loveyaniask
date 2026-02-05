using FluentValidation;
using WingsAPI.Scripting.Object.Dungeon;
using WingsAPI.Scripting.Validator.Common;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;

namespace Plugin.Act4.Scripting.Validator;

public class SDungeonValidator : AbstractValidator<SDungeon>
{
    public SDungeonValidator(IMapManager mapManager, INpcMonsterManager npcManager, IItemsManager itemsManager)
    {
        RuleFor(x => x.Maps).NotEmpty();
        RuleForEach(x => x.Maps).SetValidator(new SMapValidator(mapManager, npcManager, itemsManager));

        RuleFor(x => x.Spawn).SetValidator(new SLocationValidator());
        RuleFor(x => x.DungeonType).IsInEnum();
    }
}