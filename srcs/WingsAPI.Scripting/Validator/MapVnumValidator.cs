using FluentValidation;
using WingsEmu.Game.Maps;

namespace WingsAPI.Scripting.Validator
{
    public class MapVnumValidator : AbstractValidator<int>
    {
        public MapVnumValidator(IMapManager manager)
        {
            RuleFor(x => x).Must(x => manager.GetMapByMapId(x) != null);
        }
    }
}