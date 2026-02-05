using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace Plugin.Alzanor.Managers;

public interface IAlzanorFactory
{
    Task<AlzanorParty> CreateAlzanorEvent(List<IClientSession> redTeam, List<IClientSession> blueTeam);
}

public class AlzanorFactory : IAlzanorFactory
{
    private readonly IMapManager _mapManager;
    private readonly INpcEntityFactory _npcEntityFactory;
    private readonly AlzanorConfiguration _alzanorConfiguration;
    private readonly IMonsterEntityFactory _entity;
    private readonly IAlzanorManager _alzanorManager;

    public AlzanorFactory(IMapManager mapManager, INpcEntityFactory npcEntityFactory, AlzanorConfiguration alzanorConfiguration, IMonsterEntityFactory entity, IAlzanorManager alzanorManager)
    {
        _mapManager = mapManager;
        _npcEntityFactory = npcEntityFactory;
        _alzanorConfiguration = alzanorConfiguration;
        _entity = entity;
        _alzanorManager = alzanorManager;
    }

    public async Task<AlzanorParty> CreateAlzanorEvent(List<IClientSession> redTeam, List<IClientSession> blueTeam)
    {
        IMapInstance mapInstance = _mapManager.GenerateMapInstanceByMapVNum(new ServerMapDto
        {
            MapVnum = _alzanorConfiguration.MapId,
        }, MapInstanceType.Alzanor, []);
        
        mapInstance.Initialize(DateTime.UtcNow.AddSeconds(-1));
        _alzanorManager.AlzanorInstance = mapInstance;
        
        IMonsterEntity monsterEntity = _entity.CreateMonster((int)MonsterVnum.ALZANOR_BOSS, mapInstance, new MonsterEntityBuilder
        {
            IsWalkingAround = true,
            IsHostile = true,
            IsBoss = true,
            IsTarget = true,
            IsRespawningOnDeath = false,
            PositionX = _alzanorConfiguration.BossX,
            PositionY = _alzanorConfiguration.BossY,
            Direction = 2,
            HpMultiplier = 2
        });
        monsterEntity.ChangePosition(new Position(_alzanorConfiguration.BossX, _alzanorConfiguration.BossY));
        monsterEntity.FirstX = _alzanorConfiguration.BossX;
        monsterEntity.FirstY = _alzanorConfiguration.BossY;
        await monsterEntity.EmitEventAsync(new MapJoinMonsterEntityEvent(monsterEntity));

        _alzanorManager.AlzanorBoss = monsterEntity;
        
        mapInstance.IsPvp = true;
        _alzanorManager.AlzanorStart = DateTime.UtcNow;
        _alzanorManager.AlzanorEnd = _alzanorManager.AlzanorStart + TimeSpan.FromMinutes(_alzanorConfiguration.DurationInMinutes);
        _alzanorManager.IsActive = true;
        
        var alzanorParty = new AlzanorParty(redTeam, blueTeam, _alzanorConfiguration)
        {
            MapInstance = mapInstance
        };
        return alzanorParty;
    }
}