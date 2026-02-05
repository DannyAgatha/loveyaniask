using FluentValidation;
using WingsAPI.Scripting.Object.Common;
using WingsAPI.Scripting.Validator.Common.Map;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;

namespace WingsAPI.Scripting.Validator.Common
{
    public class SMapValidator : AbstractValidator<SMap>
    {
        public SMapValidator(IMapManager mapManager, INpcMonsterManager npcManager, IItemsManager itemsManager)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.MapIdVnum).SetValidator(new MapVnumValidator(mapManager));
            RuleForEach(x => x.Npcs).SetValidator(new SNpcValidator(npcManager));
            RuleForEach(x => x.Monsters).SetValidator(new SMonsterValidator(npcManager));
            RuleForEach(x => x.Objects).SetValidator(new SMapObjectValidator(itemsManager));
            RuleForEach(x => x.Portals).SetValidator(new SPortalValidator());
        }
    }
}