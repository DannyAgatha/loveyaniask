using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WingsEmu.Core.Generics;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.TimeSpaces;

public class TimeSpaceParty
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<IClientSession> _members;

    public TimeSpaceParty(TimeSpaceFileConfiguration timeSpaceInformation, bool isEasyMode, bool isChallengeMode)
    {
        Id = Guid.NewGuid();
        TimeSpaceInformation = timeSpaceInformation;
        IsEasyMode = isEasyMode;
        IsChallengeMode = isChallengeMode;
        TimeSpaceId = timeSpaceInformation.TsId;
        _members = new List<IClientSession>(timeSpaceInformation.MaxPlayers);
    }

    public Guid Id { get; }
    public bool Entered { get; private set; }
    public bool Started { get; private set; }
    public bool Finished { get; private set; }
    public bool Destroy { get; set; }
    public long TimeSpaceId { get; }

    public IReadOnlyList<IClientSession> Members
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _members.FindAll(x => x != null);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public TimeSpaceInstance Instance { get; private set; }
    public IClientSession Leader => Members.FirstOrDefault();
    public ThreadSafeHashSet<long> ClaimedRewards { get; } = new();
    public ThreadSafeHashSet<long> FirstCompletedTimeSpaceIds { get; } = new();
    public TimeSpaceFileConfiguration TimeSpaceInformation { get; }
    public bool IsEasyMode { get; }
    public bool IsChallengeMode { get; }
    public byte HigherLevel { get; set; }
    public DateTime LastObjectivesCheck { get; set; }
    public int? ItemVnumToRemove { get; set; }

    public void SetEnteredTimeSpace(TimeSpaceInstance timeSpace)
    {
        Entered = true;
        Instance = timeSpace;
    }

    public void StartTimeSpace()
    {
        Started = true;
        Instance.UpdateFinishDate();
    }

    public void AddMember(IClientSession session)
    {
        _lock.EnterWriteLock();
        try
        {
            _members.Add(session);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void RemoveMember(IClientSession session)
    {
        _lock.EnterWriteLock();
        try
        {
            _members.Remove(session);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void FinishTimeSpace(DateTime toRemove)
    {
        Finished = true;
        Instance?.SetDestroyDate(toRemove);
    }
}