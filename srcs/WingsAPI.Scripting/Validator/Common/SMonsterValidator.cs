using FluentValidation;
using WingsAPI.Scripting.Object.Common;
using WingsEmu.Game.Managers.StaticData;

namespace WingsAPI.Scripting.Validator.Common
{
    public class SMonsterValidator : AbstractValidator<SMonster>
    {
        public SMonsterValidator(INpcMonsterManager manager)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Position).NotNull();
            RuleFor(x => x.Vnum).SetValidator(new MonsterVnumValidator(manager));
            RuleFor(x => x.Events).NotNull();
        }
    }
}