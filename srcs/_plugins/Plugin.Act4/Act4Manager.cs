using System;
using System.Collections.Concurrent;
using WingsAPI.Packets.Enums.Act4;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4;

public class Act4Manager : IAct4Manager
{
    private readonly Act4Configuration _act4Configuration;
    private readonly IAct4DungeonManager _act4DungeonManager;
    private readonly ConcurrentQueue<(FactionType factionType, int amount)> _pointsToAdd = new();
    private int _angelPoints;
    private int _demonPoints;
    private IMonsterEntity _mukraju;

    private DateTime _mukrajuDeleteTime;
    private FactionType _mukrajuFaction = FactionType.Neutral;

    public Act4Manager(Act4Configuration act4Configuration, IAct4DungeonManager act4DungeonManager)
    {
        _act4Configuration = act4Configuration;
        _act4DungeonManager = act4DungeonManager;
    }

    public void AddFactionPoints(FactionType factionType, int amount)
    {
        if (FactionPointsLocked)
        {
            return;
        }

        _pointsToAdd.Enqueue((factionType, amount));
    }

    public void ResetFactionPoints(FactionType factionType)
    {
        if (factionType == FactionType.Angel)
        {
            _angelPoints = default;
        }
        else
        {
            _demonPoints = default;
        }
    }

    public bool FactionPointsLocked => _mukraju != null || _act4DungeonManager.DungeonsActive;

    public void RegisterMukraju(DateTime current, IMonsterEntity mukraju, FactionType factionType)
    {
        _mukrajuDeleteTime = current + _act4Configuration.MukrajuEndSpan;
        _mukraju = mukraju;
        _mukrajuFaction = factionType;
    }

    public (DateTime deleteTime, IMonsterEntity mukraju, FactionType mukrajuFactionType) GetMukraju() => (_mukrajuDeleteTime, _mukraju, _mukrajuFaction);

    public IMonsterEntity UnregisterMukraju()
    {
        IMonsterEntity monsterEntity = _mukraju;
        _mukraju = null;
        _mukrajuFaction = FactionType.Neutral;
        return monsterEntity;
    }

    public FactionType MukrajuFaction() => _mukrajuFaction;

    public FactionType GetTriumphantFaction()
    {
        if (_pointsToAdd.IsEmpty)
        {
            return FactionType.Neutral;
        }

        while (_pointsToAdd.TryDequeue(out (FactionType factionType, int amount) pointsToAdd))
        {
            if (pointsToAdd.factionType == FactionType.Angel)
            {
                _angelPoints += pointsToAdd.amount;
                if (_act4Configuration.MaximumFactionPoints > _angelPoints)
                {
                    continue;
                }

                _pointsToAdd.Clear();
                _angelPoints = default;
                return FactionType.Angel;
            }

            _demonPoints += pointsToAdd.amount;
            if (_act4Configuration.MaximumFactionPoints > _demonPoints)
            {
                continue;
            }

            _pointsToAdd.Clear();
            _demonPoints = default;
            return FactionType.Demon;
        }

        return FactionType.Neutral;
    }

    public Act4Status GetStatus()
    {
        float maxPoints = _act4Configuration.MaximumFactionPoints;

        DateTime todayResetDate = DateTime.Today + _act4Configuration.ResetDate;
        DateTime currentDate = DateTime.UtcNow;
        TimeSpan timeSpan;

        if (todayResetDate <= currentDate)
        {
            timeSpan = todayResetDate.AddDays(1) - currentDate;
        }
        else
        {
            timeSpan = todayResetDate - currentDate;
        }

        FactionType relevantFaction = FactionType.Neutral;
        Act4FactionStateType factionStateType = Act4FactionStateType.Nothing;
        TimeSpan currentTimeBeforeX = TimeSpan.Zero;
        TimeSpan timeBeforeX = TimeSpan.Zero;

        (DateTime deleteTime, IMonsterEntity mukraju, FactionType mukrajuFactionType) = GetMukraju();
        if (mukraju != null)
        {
            relevantFaction = mukrajuFactionType;
            factionStateType = Act4FactionStateType.Mukraju;
            currentTimeBeforeX = deleteTime - currentDate;
            timeBeforeX = _act4Configuration.MukrajuEndSpan;
        }

        DungeonType dungeonType = DungeonType.Morcos;

        if (_act4DungeonManager.DungeonsActive)
        {
            relevantFaction = _act4DungeonManager.AllowedFaction;
            factionStateType = Act4FactionStateType.RaidDungeon;
            currentTimeBeforeX = _act4DungeonManager.DungeonEnd - currentDate;
            timeBeforeX = _act4DungeonManager.DungeonEndSpan;
            dungeonType = _act4DungeonManager.DungeonType;
        }

        return new Act4Status(Convert.ToByte(_angelPoints / maxPoints * 100), Convert.ToByte(_demonPoints / maxPoints * 100), timeSpan,
            relevantFaction, factionStateType, currentTimeBeforeX, timeBeforeX, dungeonType);
    }
}