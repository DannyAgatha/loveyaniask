using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorKillBonusEventHandler : IAsyncEventProcessor<KillBonusEvent>
{
    private readonly IAlzanorManager _alzanorManager;
    private readonly IAsyncEventPipeline _eventPipeline;
    
    public AlzanorKillBonusEventHandler(IAlzanorManager alzanorManager, IAsyncEventPipeline eventPipeline)
    {
        _alzanorManager = alzanorManager;
        _eventPipeline = eventPipeline;
    }

    public async Task HandleAsync(KillBonusEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monsterEntityToAttack = e.MonsterEntity;
        AlzanorParty party = e.Sender.PlayerEntity.AlzanorComponent.AlzanorParty;
        if(party == null)
        {
            return;
        }

        if (monsterEntityToAttack == null || monsterEntityToAttack.IsStillAlive)
        {
            return;
        }

        if (e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.Alzanor || monsterEntityToAttack.MonsterVNum != (int)MonsterVnum.ALZANOR_BOSS)
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new AlzanorEndEvent
        {
            AlzanorParty = party
        }, cancellation);
    }
}