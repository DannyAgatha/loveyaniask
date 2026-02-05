using FluentValidation;
using WingsEmu.Game.Managers.StaticData;

namespace WingsAPI.Scripting.Validator
{
    public class ItemVnumValidator : AbstractValidator<short>
    {
        public ItemVnumValidator(IItemsManager manager)
        {
            RuleFor(x => x).Must(x => manager.GetItem(x) != null);
        }
    }
}