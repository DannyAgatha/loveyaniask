using System;
using System.Collections.Generic;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Raids;

public class RaidInstance
{
    public RaidInstance(IReadOnlyCollection<RaidSubInstance> raidSubInstances, RaidSubInstance spawnInstance, Position spawnPoint, TimeSpan timeToComplete, byte maxLives, RaidReward raidReward)
    {
        foreach (RaidSubInstance raidSubInstance in raidSubInstances)
        {
            raidSubInstance.MapInstance.Initialize(DateTime.UtcNow.AddMilliseconds(-500));
        }

        DateTime now = DateTime.UtcNow;

        RaidSubInstances = GetDictionary(raidSubInstances);
        FinishDate = now.Add(timeToComplete);
        StartDate = now;
        RemoveDate = FinishDate;
        Lives = maxLives;
        MaxLives = maxLives;
        SpawnPoint = spawnPoint;
        SpawnInstance = spawnInstance;
        RaidReward = raidReward;
    }

    public IReadOnlyDictionary<Guid, RaidSubInstance> RaidSubInstances { get; }

    public int Lives { get; private set; }

    public byte MaxLives { get; }

    public Position SpawnPoint { get; }

    public RaidSubInstance SpawnInstance { get; }

    public RaidReward RaidReward { get; }

    public DateTime FinishDate { get; }

    public DateTime StartDate { get; }

    public DateTime RemoveDate { get; private set; }

    public DateTime? FinishSlowMoDate { get; private set; }

    public TimeSpan TimeUntilEnd => FinishDate - DateTime.UtcNow;

    private static IReadOnlyDictionary<Guid, RaidSubInstance> GetDictionary(IEnumerable<RaidSubInstance> raidSubInstances)
    {
        var dictionary = new Dictionary<Guid, RaidSubInstance>();
        foreach (RaidSubInstance raidSubInstance in raidSubInstances)
        {
            dictionary.TryAdd(raidSubInstance.MapInstance.Id, raidSubInstance);
        }

        return dictionary;
    }

    public void IncreaseOrDecreaseLives(short amount)
    {
        int futureValue = Lives + amount;
        if (futureValue > MaxLives)
        {
            Lives = MaxLives;
            return;
        }

        Lives = futureValue;
    }

    public void SetDestroyDate(DateTime dateTime)
    {
        RemoveDate = dateTime;
    }

    public void SetFinishSlowMoDate(DateTime? dateTime)
    {
        FinishSlowMoDate = dateTime;
    }
}