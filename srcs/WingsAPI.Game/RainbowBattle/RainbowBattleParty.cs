using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.RainbowBattle;

public class RainbowBattleParty
{
    private readonly List<IClientSession> _blueTeam;

    private readonly ReaderWriterLockSlim _lock = new();

    private readonly List<IClientSession> _redTeam;

    public RainbowBattleParty(List<IClientSession> redTeam, List<IClientSession> blueTeam)
    {
        DateTime now = DateTime.UtcNow;
        Id = Guid.NewGuid();
        _redTeam = redTeam;
        _blueTeam = blueTeam;
        EndTime = now.AddMinutes(5);
        StartTime = now;
        DragonKilledCount = 0;
    }

    public Guid Id { get; }

    public DateTime EndTime { get; }
    public DateTime StartTime { get; }
    public DateTime LastMembersLife { get; set; }
    public DateTime? FinishTime { get; set; }
    public DateTime LastPointsTeamAdd { get; set; }
    public DateTime LastMandragoreSpawn { get; set; }
    public DateTime LastActivityPointsTeamAdd { get; set; }
    public bool Started { get; set; }

    public IMapInstance MapInstance { get; init; }

    public int RedPoints { get; private set; }
    public int BluePoints { get; private set; }
    public int DragonKilledCount { get; set; }

    public ConcurrentDictionary<RainbowBattleFlagType, byte> RedFlags { get; set; } = new();
    public ConcurrentDictionary<RainbowBattleFlagType, byte> BlueFlags { get; set; } = new();

    public IReadOnlyList<IClientSession> RedTeam
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _redTeam.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public IReadOnlyList<IClientSession> BlueTeam
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _blueTeam.ToArray();
                ;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public void IncreaseRedPoints(int count)
    {
        _lock.EnterWriteLock();
        try
        {
            RedPoints += count;

            if (RedPoints < 0)
            {
                RedPoints = 0;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void IncreaseBluePoints(int count)
    {
        _lock.EnterWriteLock();
        try
        {
            BluePoints += count;

            if (BluePoints < 0)
            {
                BluePoints = 0;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveRedPlayer(IClientSession session)
    {
        _lock.EnterWriteLock();
        try
        {
            _redTeam.Remove(session);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveBluePlayer(IClientSession session)
    {
        _lock.EnterWriteLock();
        try
        {
            _blueTeam.Remove(session);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}