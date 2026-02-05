using FluentValidation;
using WingsAPI.Scripting.Object.Timespace;
using WingsAPI.Scripting.Validator.Common;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;

namespace Plugin.TimeSpaces;

public class STimeSpaceValidator : AbstractValidator<ScriptTimeSpace>
{
    public STimeSpaceValidator(IMapManager mapManager, INpcMonsterManager npcManager, IItemsManager itemsManager)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Maps).NotEmpty();
        RuleFor(x => x.Spawn).SetValidator(new SLocationValidator());
        RuleForEach(x => x.Maps).SetValidator(new SMapValidator(mapManager, npcManager, itemsManager));
    }
}