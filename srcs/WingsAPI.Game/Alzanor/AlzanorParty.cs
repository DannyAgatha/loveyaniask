using System;
using System.Collections.Generic;
using System.Threading;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Alzanor;

public class AlzanorParty
{
    private readonly AlzanorConfiguration _alzanorConfiguration;
    private readonly List<IClientSession> _blueTeam;

    private readonly ReaderWriterLockSlim _lock = new();

    private readonly List<IClientSession> _redTeam;

    public AlzanorParty(List<IClientSession> redTeam, List<IClientSession> blueTeam, AlzanorConfiguration alzanorConfiguration)
    {
        DateTime now = DateTime.UtcNow;
        Id = Guid.NewGuid();
        _redTeam = redTeam;
        _blueTeam = blueTeam;
        _alzanorConfiguration = alzanorConfiguration;
        EndTime = now.AddMinutes(_alzanorConfiguration.DurationInMinutes);
        StartTime = now;
    }
    public Guid Id { get; }
    public DateTime EndTime { get; }
    public DateTime StartTime { get; }
    public DateTime LastMembersLife { get; set; }
    public DateTime? FinishTime { get; set; }
    public DateTime LastPointsTeamAdd { get; set; }
    public bool Started { get; set; }
    public AlzanorTeamType? Winner { get; set; }
    public IMapInstance MapInstance { get; init; }

    public int RedPoints { get; private set; }
    public int BluePoints { get; private set; }
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