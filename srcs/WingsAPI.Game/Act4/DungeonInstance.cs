using System;
using System.Collections.Generic;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Raids;

namespace WingsEmu.Game.Act4;

public class DungeonInstance
{
    public DungeonInstance(long familyId, DungeonType dungeonType, IReadOnlyCollection<DungeonSubInstance> dungeonSubInstances,
        DungeonSubInstance spawnInstance, Position spawnPoint, RaidReward raidReward)
    {
        foreach (DungeonSubInstance dungeonSubInstance in dungeonSubInstances)
        {
            dungeonSubInstance.MapInstance.Initialize(DateTime.UtcNow.AddMilliseconds(-500));
        }

        FamilyId = familyId;
        DungeonType = dungeonType;
        DungeonSubInstances = GetDictionary(dungeonSubInstances);
        SpawnInstance = spawnInstance;
        SpawnPoint = spawnPoint;
        DungeonReward = raidReward;
    }

    public long FamilyId { get; }
    public DungeonType DungeonType { get; }
    public IReadOnlyDictionary<Guid, DungeonSubInstance> DungeonSubInstances { get; }
    public Position SpawnPoint { get; }
    public DungeonSubInstance SpawnInstance { get; }
    public RaidReward DungeonReward { get; }
    public bool PlayerDeathInBossRoom { get; set; }
    public DateTime StartInBoosRoom { get; set; }

    public DateTime? FinishSlowMoDate { get; set; }
    public DateTime? CleanUpBossMapDate { get; set; }

    private static IReadOnlyDictionary<Guid, DungeonSubInstance> GetDictionary(IEnumerable<DungeonSubInstance> dungeonSubInstances)
    {
        var dictionary = new Dictionary<Guid, DungeonSubInstance>();
        foreach (DungeonSubInstance dungeonSubInstance in dungeonSubInstances)
        {
            dictionary.TryAdd(dungeonSubInstance.MapInstance.Id, dungeonSubInstance);
        }

        return dictionary;
    }
}