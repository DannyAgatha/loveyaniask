using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalDamageDealtEventHandler : IAsyncEventProcessor<ApplyHitEvent>
{
    private readonly ISkillUsageManager _skillUsageManager;

    public TotalDamageDealtEventHandler(ISkillUsageManager skillUsageManager) => _skillUsageManager = skillUsageManager;

    public async Task HandleAsync(ApplyHitEvent e, CancellationToken cancellation)
    {
        if (e.HitInformation.Caster is not IPlayerEntity playerEntity)
        {
            return;
        }

        SkillInfo skill = e.HitInformation.Skill;
        long totalDamage = e.ProcessResults.Damages;

        if (skill.Combos.Any())
        {
            ComboState comboState = _skillUsageManager.GetComboState(e.HitInformation.Caster.Id, e.Target.Id);
            double increaseDamageByComboState = 0.05 + 0.1 * comboState.Hit;

            totalDamage += (int)(totalDamage * increaseDamageByComboState);
        }

        playerEntity.LifetimeStats.TotalDamageDealt += totalDamage;
    }
}