using System;
using System.Collections.Generic;
using WingsEmu.Game;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4;

public class Act4DungeonManager : IAct4DungeonManager
{
    private readonly Act4DungeonsConfiguration _act4DungeonsConfiguration;

    private readonly List<DungeonInstance> _dungeons = new();
    private readonly Dictionary<long, DungeonInstance> _dungeonsByFamilyId = new();

    private IReadOnlyList<IMonsterEntity> _guardians;
    private IPortalEntity _portal;

    public Act4DungeonManager(Act4DungeonsConfiguration act4DungeonsConfiguration) => _act4DungeonsConfiguration = act4DungeonsConfiguration;

    public bool DungeonsActive { get; private set; }
    public DungeonType DungeonType { get; private set; }
    public FactionType AllowedFaction { get; private set; }
    public DateTime DungeonEnd { get; private set; }
    public DateTime DungeonStart { get; private set; }
    public IReadOnlyList<DungeonInstance> Dungeons => _dungeons;
    public TimeSpan DungeonEndSpan => _act4DungeonsConfiguration.DungeonDuration;

    public void EnableDungeons(DungeonType dungeonType, FactionType allowedFaction)
    {
        if (DungeonsActive)
        {
            return;
        }

        DungeonsActive = true;
        DungeonType = dungeonType;
        AllowedFaction = allowedFaction;
        DateTime currentTime = DateTime.UtcNow;
        DungeonEnd = currentTime.Add(DungeonEndSpan);
        DungeonStart = currentTime;
    }

    public void SetGuardiansAndPortal(IReadOnlyList<IMonsterEntity> guardians, IPortalEntity portal)
    {
        _guardians = guardians;
        _portal = portal;
    }

    public (IReadOnlyList<IMonsterEntity>, IPortalEntity) GetAndCleanGuardians()
    {
        IReadOnlyList<IMonsterEntity> list = _guardians;
        IPortalEntity portal = _portal;
        _guardians = null;
        _portal = null;
        return (list, portal);
    }

    public void DisableDungeons()
    {
        DungeonsActive = false;
    }

    public void RegisterDungeon(DungeonInstance dungeonInstance)
    {
        if (!DungeonsActive || dungeonInstance.DungeonType != DungeonType || _dungeonsByFamilyId.ContainsKey(dungeonInstance.FamilyId))
        {
            return;
        }

        _dungeons.Add(dungeonInstance);
        _dungeonsByFamilyId.Add(dungeonInstance.FamilyId, dungeonInstance);
    }

    public void UnregisterDungeon(DungeonInstance dungeonInstance)
    {
        _dungeons.Remove(dungeonInstance);
        _dungeonsByFamilyId.Remove(dungeonInstance.FamilyId);
    }

    public DungeonInstance GetDungeon(long familyId) => _dungeonsByFamilyId.TryGetValue(familyId, out DungeonInstance dungeon) ? dungeon : null;
}