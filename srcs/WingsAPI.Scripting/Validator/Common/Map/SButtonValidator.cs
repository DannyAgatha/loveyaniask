using FluentValidation;
using WingsAPI.Scripting.Object.Common.Map;
using WingsEmu.Game.Managers.StaticData;

namespace WingsAPI.Scripting.Validator.Common.Map
{
    public class SButtonValidator : AbstractValidator<SButton>
    {
        public SButtonValidator(IItemsManager manager)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Position).NotNull();
            RuleFor(x => x.ActivatedVnum).NotEmpty().SetValidator(new ItemVnumValidator(manager));
            RuleFor(x => x.DeactivatedVnum).NotEmpty().SetValidator(new ItemVnumValidator(manager));
            RuleFor(x => x.Events).NotNull();
        }
    }
}