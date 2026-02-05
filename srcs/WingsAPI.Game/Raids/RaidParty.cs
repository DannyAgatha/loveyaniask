using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WingsAPI.Packets.Enums;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Raids;

public class RaidParty
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<IClientSession> _members;

    public RaidParty(Guid id, RaidType type,
        byte minimumLevel, byte maximumLevel,
        byte minimumHeroLevel, byte maximumHeroLevel,
        byte minimumMembers, byte maximumMembers, bool isMarathonMode = false)
    {
        Id = id;
        Type = type;
        MinimumLevel = minimumLevel;
        MaximumLevel = maximumLevel;
        MinimumHeroLevel = minimumHeroLevel;
        MaximumHeroLevel = maximumHeroLevel;
        MinimumMembers = minimumMembers;
        MaximumMembers = maximumMembers;
        IsMarathonMode = isMarathonMode;
        _members = new List<IClientSession>(MaximumMembers);
    }

    public Guid Id { get; }

    public bool Started { get; private set; }
    public bool Finished { get; private set; }
    public bool Destroy { get; set; }
    public RaidType Type { get; }
    public byte MinimumLevel { get; }
    public byte MaximumLevel { get; }
    public byte MinimumHeroLevel { get; }
    public byte MaximumHeroLevel { get; }
    public byte MaximumMembers { get; }
    public byte MinimumMembers { get; }
    
    public bool IsSpecialRaid() => _specialRaidTypes.Contains(this.Type);

    public bool IsMarathonMode { get; set; }
    
    private readonly HashSet<RaidType> _specialRaidTypes =
    [
        RaidType.Laurena,
        RaidType.Belial,
        RaidType.Zenas,
        RaidType.OverlordPaimon
    ];
    public ModeType ModeType { get; set; }
    
    
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

    public RaidInstance Instance { get; private set; }
    
    public DateTime StartTime { get; set; }

    public IClientSession Leader
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _members.FirstOrDefault();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public static IEqualityComparer<RaidParty> IdComparer { get; } = new IdEqualityComparer();
    
    public RaidParty Clone()
    {
        var clone = new RaidParty(
            Id,
            Type,
            MinimumLevel,
            MaximumLevel,
            MinimumHeroLevel,
            MaximumHeroLevel,
            MinimumMembers,
            MaximumMembers,
            IsMarathonMode
        )
        {
            Started = Started,
            Finished = Finished,
            Destroy = Destroy
        };

        foreach (IClientSession member in Members)
        {
            clone.AddMember(member);
        }

        return clone;
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

    public void StartRaid(RaidInstance raid)
    {
        Started = true;
        Instance = raid;
    }

    public void FinishRaid(DateTime toRemove)
    {
        Finished = true;
        Instance?.SetDestroyDate(toRemove);
    }

    protected bool Equals(RaidParty other) => Id.Equals(other.Id);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((RaidParty)obj);
    }

    public override int GetHashCode() => Id.GetHashCode();


    private sealed class IdEqualityComparer : IEqualityComparer<RaidParty>
    {
        public bool Equals(RaidParty x, RaidParty y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(RaidParty obj) => obj.Id.GetHashCode();
    }
}