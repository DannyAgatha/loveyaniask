using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Npcs.Event;

namespace NosEmu.Plugins.BasicImplementations.Entities;

public class GenerateEntityDeathEventHandler : IAsyncEventProcessor<GenerateEntityDeathEvent>
{
    private readonly IAsyncEventPipeline _eventPipeline;

    public GenerateEntityDeathEventHandler(IAsyncEventPipeline eventPipeline) => _eventPipeline = eventPipeline;

    public async Task HandleAsync(GenerateEntityDeathEvent e, CancellationToken cancellation)
    {
        IBattleEntity defender = e.Entity;
        IBattleEntity attacker = e.Attacker;

        defender.Hp = 0;
        switch (defender)
        {
            case IPlayerEntity character:
                await character.Session.EmitEventAsync(new PlayerDeathEvent
                {
                    Killer = attacker
                });
                break;
            case IMateEntity mate:
                await mate.Owner.Session.EmitEventAsync(new MateDeathEvent(attacker, mate));
                break;
            case INpcEntity npc:
                await _eventPipeline.ProcessEventAsync(new MapNpcGenerateDeathEvent(npc, attacker));
                break;
            case IMonsterEntity monster:
                await _eventPipeline.ProcessEventAsync(new MonsterDeathEvent(monster)
                {
                    Killer = attacker
                });
                break;
        }

        if (e.IsByMainWeapon is null)
        {
            return;
        }

        if (attacker is not IPlayerEntity playerEntity)
        {
            return;
        }

        if (!playerEntity.IsAlive())
        {
            return;
        }

        if (HasLevelPenalty(attacker, defender))
        {
            return;
        }

        int hpHeal = playerEntity.GetMaxWeaponShellValue(ShellEffectType.HPRecoveryForKilling, e.IsByMainWeapon.Value);

        if (hpHeal != 0)
        {
            await playerEntity.EmitEventAsync(new BattleEntityHealEvent
            {
                Entity = playerEntity,
                HpHeal = hpHeal
            });
        }

        int mpHeal = playerEntity.GetMaxWeaponShellValue(ShellEffectType.MPRecoveryForKilling, e.IsByMainWeapon.Value);
        if (mpHeal == 0)
        {
            return;
        }

        await playerEntity.EmitEventAsync(new BattleEntityHealEvent
        {
            Entity = playerEntity,
            MpHeal = mpHeal
        });
    }

    private bool HasLevelPenalty(IBattleEntity attacker, IBattleEntity target)
    {
        if (target.Level >= 75)
        {
            return false;
        }

        return attacker.Level - target.Level > 15;
    }
}