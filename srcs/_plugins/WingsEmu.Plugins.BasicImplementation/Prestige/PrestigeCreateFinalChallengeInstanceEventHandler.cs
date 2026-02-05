using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Configurations.Prestige;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Prestige;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Prestige;

public class PrestigeCreateFinalChallengeInstanceEventHandler : IAsyncEventProcessor<PrestigeCreateFinalChallengeInstanceEvent>
{
    private readonly IMapManager _mapManager;
    private readonly IMonsterEntityFactory _monsterEntityFactory;

    public PrestigeCreateFinalChallengeInstanceEventHandler(IMapManager mapManager, IMonsterEntityFactory monsterEntityFactory)
    {
        _mapManager = mapManager;
        _monsterEntityFactory = monsterEntityFactory;
    }

    public async Task HandleAsync(PrestigeCreateFinalChallengeInstanceEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        ChallengeMap mapConfig = e.FinalChallenge.Map;

        IMapInstance instance = _mapManager.GeneratePrestigeMapInstance(mapConfig.Vnum);
        if (instance == null)
        {
            session.SendInfo("Unable to create the final challenge instance.");
            return;
        }
        
        var prestigeInstance = new PrestigeInstance(
            instance,
            e.FinalChallenge.TimeLimitMinutes,
            e.FinalChallenge.WarningMilestones
        );
        PrestigeInstanceManager.PrestigeInstances[instance] = prestigeInstance;


        session.ChangeMap(instance, mapConfig.PlayerSpawnX, mapConfig.PlayerSpawnY);
        session.SendInfo("The prestige challenge has begun!");

        IMonsterEntity boss = _monsterEntityFactory.CreateMonster(e.FinalChallenge.BossMonsterVnum, instance, new MonsterEntityBuilder
        {
            IsWalkingAround = true,
            IsHostile = true,
            IsBoss = true,
            IsTarget = true,
            IsRespawningOnDeath = false,
            PositionX = mapConfig.MonsterSpawnX,
            PositionY = mapConfig.MonsterSpawnY
        });

        await boss.EmitEventAsync(new MapJoinMonsterEntityEvent(boss, mapConfig.MonsterSpawnX, mapConfig.MonsterSpawnY));

        int limitMinutes = e.FinalChallenge.TimeLimitMinutes;
        var totalTime = TimeSpan.FromMinutes(limitMinutes);
        
        instance.Broadcast(x => x.GenerateClockPacket(ClockType.RedMiddle, 0, totalTime, totalTime));
    }
}