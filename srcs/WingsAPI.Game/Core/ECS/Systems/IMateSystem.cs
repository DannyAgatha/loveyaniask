using System;
using System.Collections.Generic;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;

namespace WingsEmu.Game._ECS.Systems;

public interface IMateSystem
{
    IReadOnlyList<IMateEntity> GetAliveMates();
    IReadOnlyList<IMateEntity> GetAliveMates(Func<IMateEntity, bool> predicate);
    IReadOnlyList<IMateEntity> GetAliveMatesInRange(Position position, short range);
    IReadOnlyList<IMateEntity> GetClosestMatesInRange(Position position, short range);
    IReadOnlyList<IMateEntity> GetAliveMatesInRange(Position position, short range, Func<IBattleEntity, bool> predicate);
    IMateEntity GetMateById(long mateId);
    void AddMate(IMateEntity entity);
    void RemoveMate(IMateEntity entity);
}