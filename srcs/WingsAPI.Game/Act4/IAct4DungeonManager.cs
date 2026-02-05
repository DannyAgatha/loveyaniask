using System;
using System.Collections.Generic;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act4;

public interface IAct4DungeonManager
{
    public bool DungeonsActive { get; }
    public DungeonType DungeonType { get; }
    public FactionType AllowedFaction { get; }
    public DateTime DungeonEnd { get; }
    public TimeSpan DungeonEndSpan { get; }
    public DateTime DungeonStart { get; }
    public IReadOnlyList<DungeonInstance> Dungeons { get; }

    public void EnableDungeons(DungeonType dungeonType, FactionType allowedFaction);
    public void SetGuardiansAndPortal(IReadOnlyList<IMonsterEntity> guardians, IPortalEntity portal);
    public (IReadOnlyList<IMonsterEntity>, IPortalEntity) GetAndCleanGuardians();
    public void DisableDungeons();

    public void RegisterDungeon(DungeonInstance dungeonInstance);
    public void UnregisterDungeon(DungeonInstance dungeonInstance);
    public DungeonInstance GetDungeon(long familyId);
}