using FluentValidation;
using WingsAPI.Scripting.Object.Common.Map;
using WingsEmu.Game.Managers.StaticData;

namespace WingsAPI.Scripting.Validator.Common.Map
{
    public class SMapObjectValidator : AbstractValidator<SMapObject>
    {
        public SMapObjectValidator(IItemsManager itemsManager)
        {
        }
    }
}