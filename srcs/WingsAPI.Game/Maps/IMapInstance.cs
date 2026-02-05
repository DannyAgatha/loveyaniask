using System;
using System.Collections.Generic;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.Game._ECS;
using WingsEmu.Game._ECS.Systems;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Maps;

public interface IEntityIdManager
{
    int GenerateEntityId();
}

public interface IMapInstance : IMonsterSystem, IBroadcaster, ITickProcessable, ICharacterSystem, IMateSystem, IDropSystem, INpcSystem, IBattleSystem, IEntityIdManager
{
    Guid Id { get; }
    MapInstanceType MapInstanceType { get; }

    public IReadOnlyList<byte> Grid { get; }
    public int Width { get; }
    public int Height { get; }
    public int MapId { get; }
    public int Music { get; }
    public int MapVnum { get; }
    public int MapNameId { get; }
    
    public bool IsBossRoom { get;  }
    byte MapIndexX { get; set; }
    byte MapIndexY { get; set; }

    bool IsDance { get; set; }
    short? MapMusic { get; set; }
    bool IsPvp { get; set; }
    bool ShopAllowed { get; }
    bool AIDisabled { get; set; }
    bool IsAct6PvpInstance { get; set; }
    List<IPortalEntity> Portals { get; }
    List<ITimeSpacePortalEntity> TimeSpacePortals { get; }
    List<MapDesignObject> MapDesignObjects { get; }

    public Position GetRandomPosition();
    IReadOnlyList<string> GetEntitiesOnMapPackets(bool onlyItemsAndPortals = false);

    bool HasMapFlag(MapFlags flags);

    MapItem PutItem(ushort amount, ref GameItemInstance inv, IClientSession session);
    void DespawnAndRemoveMonster(IMonsterEntity monsterEntity);
    void DespawnMonster(IMonsterEntity monsterEntity);

    /// <summary>
    ///     Returns the characters in range
    /// </summary>
    /// <param name="position"></param>
    /// <param name="distance"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IReadOnlyList<IBattleEntity> GetNonMonsterBattleEntitiesInRange(Position pos, short distance);

    IReadOnlyList<IBattleEntity> GetNonMonsterBattleEntitiesInRange(Position pos, short distance, Func<IBattleEntity, bool> predicate);
    IReadOnlyList<IBattleEntity> GetNonMonsterBattleEntities();
    IReadOnlyList<IBattleEntity> GetNonMonsterBattleEntities(Func<IBattleEntity, bool> predicate);
    IReadOnlyList<IBattleEntity> GetBattleEntities(Func<IBattleEntity, bool> predicate);
    IReadOnlyList<IBattleEntity> GetBattleEntitiesInRange(Position pos, short distance);
    IReadOnlyList<IBattleEntity> GetClosestBattleEntitiesInRange(Position pos, short distance);
    IReadOnlyList<IBattleEntity> GetPlayerBattleEntities();
    IBattleEntity GetBattleEntity(VisualType type, long id);
    void RegisterSession(IClientSession session);
    void UnregisterSession(IClientSession session);
    void LoadPortals(IEnumerable<PortalDTO> value);

    void Initialize(DateTime date);
    void Destroy();
}