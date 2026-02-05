using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core.Generics;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.TimeSpaces;

public class TimeSpaceInstance
{
    public TimeSpaceInstance(IReadOnlyCollection<TimeSpaceSubInstance> timeSpaceSubInstances, TimeSpaceSubInstance spawnInstance, Position spawnPoint, TimeSpan timeToComplete,
        byte maxLives, TimeSpaceObjective timeSpaceObjective, int bonusPointItemDropChance, HashSet<long> protectedNpcs, short? obtainablePartnerVnum,
        bool infiniteDuration, int? preFinishDialog, bool preFinishDialogIsObjective)
    {
        foreach (TimeSpaceSubInstance timeSpaceSubInstance in timeSpaceSubInstances)
        {
            timeSpaceSubInstance.MapInstance.Initialize(DateTime.UtcNow.AddMilliseconds(-500));
        }

        TimeSpaceSubInstances = timeSpaceSubInstances.ToDictionary(s => s.MapInstance.Id);
        FinishDate = DateTime.UtcNow.Add(timeToComplete);
        Lives = maxLives;
        TimeSpaceObjective = timeSpaceObjective;
        MaxLives = maxLives;
        SpawnPoint = spawnPoint;
        SpawnInstance = spawnInstance;
        TimeToComplete = timeToComplete;
        BonusPointItemDropChance = bonusPointItemDropChance;
        KilledMonsters = 0;
        SavedNpcs = 0;
        EnteredRooms = 0;
        Score = 0;
        ProtectedNpcs = protectedNpcs;
        ObtainablePartnerVnum = obtainablePartnerVnum;
        InfiniteDuration = infiniteDuration;
        PreFinishDialog = preFinishDialog;
        PreFinishDialogIsObjective = preFinishDialogIsObjective;
    }

    public IReadOnlyDictionary<Guid, TimeSpaceSubInstance> TimeSpaceSubInstances { get; }

    public int Lives { get; private set; }
    public int Score { get; private set; }
    public int KilledMonsters { get; private set; }
    public int EnteredRooms { get; private set; }
    public int SavedNpcs { get; set; }
    public byte MaxLives { get; }
    public int BonusPointItemDropChance { get; }
    public int KilledProtectedNpcs { get; set; }
    public int? PreFinishDialog { get; set; }
    public bool PreFinishDialogShown { get; set; }
    public bool PreFinishDialogIsObjective { get; set; }
    public DateTime? PreFinishDialogTime { get; set; }
    public Position SpawnPoint { get; }
    public TimeSpaceSubInstance SpawnInstance { get; }
    public DateTime FinishDate { get; private set; }
    public DateTime RemoveDate { get; private set; }
    public DateTime? StartTimeFreeze { get; set; }
    public TimeSpan TimeToComplete { get; }
    public TimeSpaceObjective TimeSpaceObjective { get; }
    public TimeSpan TimeUntilEnd => FinishDate - DateTime.UtcNow;
    public ThreadSafeHashSet<Guid> VisitedRooms { get; } = new();
    public HashSet<long> ProtectedNpcs { get; }
    public short? ObtainablePartnerVnum { get; }
    public bool InfiniteDuration { get; set; }

    public void SetDestroyDate(DateTime dateTime)
    {
        RemoveDate = dateTime;
    }

    public void UpdateFinishDate(TimeSpan? toFinish = null)
    {
        FinishDate = DateTime.UtcNow.Add(toFinish ?? TimeToComplete);
    }

    public void AddTimeToFinishDate(TimeSpan timeSpan)
    {
        FinishDate += timeSpan;
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

    public void IncreaseKilledMonsters() => KilledMonsters++;

    public void IncreaseEnteredRooms() => EnteredRooms++;

    public void IncreaseScoreByAmount(int amount) => Score += amount;

    public void UpdateFinalScore(int score) => Score = score;
}