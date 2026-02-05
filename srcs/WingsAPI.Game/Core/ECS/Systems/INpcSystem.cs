using System;
using System.Collections.Generic;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game._ECS.Systems;

public interface INpcSystem
{

    IReadOnlyList<INpcEntity> GetAliveNpcs();
    IReadOnlyList<INpcEntity> GetAliveNpcs(Func<INpcEntity, bool> predicate);
    IReadOnlyList<INpcEntity> GetPassiveNpcs();
    IReadOnlyList<INpcEntity> GetAliveNpcsInRange(Position pos, short distance, Func<INpcEntity, bool> predicate);
    IReadOnlyList<INpcEntity> GetAliveNpcsInRange(Position pos, short distance);
    IReadOnlyList<INpcEntity> GetClosestNpcsInRange(Position pos, short distance);
    void NpcRefreshTarget(INpcEntity npcEntity, IBattleEntity target);
    INpcEntity GetNpcById(long id);
    void AddNpc(INpcEntity entity);
    void RemoveNpc(INpcEntity entity);
}