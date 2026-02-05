using FluentValidation;
using WingsEmu.Game.Managers.StaticData;

namespace WingsAPI.Scripting.Validator
{
    public class MonsterVnumValidator : AbstractValidator<short>
    {
        public MonsterVnumValidator(INpcMonsterManager manager)
        {
            RuleFor(x => x).Must(x => manager.GetNpc(x) != null);
        }
    }
}