using PhoenixLib.Events;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game._enum;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorEntityDamageEventHandler : IAsyncEventProcessor<EntityDamageEvent>
{
    private readonly IAlzanorManager _alzanorManager;
    private readonly SerializableGameServer _gameServer;
    private readonly AlzanorConfiguration _alzanorConfiguration;

    public AlzanorEntityDamageEventHandler(IAlzanorManager alzanorManager, SerializableGameServer gameServer, AlzanorConfiguration alzanorConfiguration)
    {
        _alzanorManager = alzanorManager;
        _gameServer = gameServer;
        _alzanorConfiguration = alzanorConfiguration;
    }

    public async Task HandleAsync(EntityDamageEvent e, CancellationToken cancellation)
    {
        IMapInstance mapInstance = _alzanorManager.GetAlzanorInstance();
        if(mapInstance == null)
        {
            return;
        }
        
        IBattleEntity defender = e.Damaged;
        IBattleEntity attacker = e.Damager;

        if (attacker is null || defender is null)
        {
            return;
        }

        if (!attacker.IsAlive() || !defender.IsAlive())
        {
            return;
        }
        
        if (attacker.MapInstance.Id != mapInstance.Id || defender.MapInstance.Id != mapInstance.Id)
        {
            return;
        }
        
        if (defender is not IMonsterEntity { MonsterVNum: (int)MonsterVnum.ALZANOR_BOSS })
        {
            return;
        }
        
        if (attacker.AlzanorComponent.Team == AlzanorTeamType.Red)
        {
            _alzanorManager.RedDamage += e.Damage;
            return;
        }

        _alzanorManager.BlueDamage += e.Damage;
    }
}