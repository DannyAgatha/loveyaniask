using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WingsEmu.Game._enum;
using WingsEmu.Game.Configurations;

namespace WingsEmu.Game.RainbowBattle;

public interface IRainbowBattleManager
{
    bool IsActive { get; set; }
    bool IsRegistrationActive { get; }
    DateTime RegistrationStartTime { get; }
    DateTime? RainbowBattleProcessTime { get; set; }

    IReadOnlyList<(TimeSpan, int, TimeType)> Warnings { get; }
    IEnumerable<long> RegisteredPlayers { get; }

    IEnumerable<RainbowBattleParty> RainbowBattleParties { get; }
    void EnableBattleRainbowRegistration();
    void DisableBattleRainbowRegistration();

    void RegisterPlayer(long id);
    void UnregisterPlayer(long id);
    void ClearRegisteredPlayers();

    void AddRainbowBattle(RainbowBattleParty rainbowBattleParty);
    void RemoveRainbowBattle(RainbowBattleParty rainbowBattleParty);
}

public class RainbowBattleManager : IRainbowBattleManager
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly ConcurrentDictionary<Guid, RainbowBattleParty> _rainbowBattleParties = new();
    private readonly HashSet<long> _registeredPlayers = new();

    public RainbowBattleManager(RainbowBattleConfiguration rainbowBattleConfiguration)
    {
        var warnings = new List<(TimeSpan, int, TimeType)>();
        foreach (TimeSpan warning in rainbowBattleConfiguration.Warnings)
        {
            TimeSpan time = TimeSpan.FromMinutes(5) - warning;
            bool isSec = time.TotalMinutes < 1;

            warnings.Add(new ValueTuple<TimeSpan, int, TimeType>(warning, (int)(isSec ? time.TotalSeconds : time.TotalMinutes), isSec ? TimeType.SECONDS : TimeType.MINUTES));
        }

        Warnings = warnings;
    }

    public bool IsActive { get; set; }
    public bool IsRegistrationActive { get; private set; }

    public void EnableBattleRainbowRegistration()
    {
        IsRegistrationActive = true;
        RegistrationStartTime = DateTime.UtcNow.AddSeconds(30);
    }

    public void DisableBattleRainbowRegistration()
    {
        IsRegistrationActive = false;
        RegistrationStartTime = DateTime.MinValue;
    }

    public DateTime RegistrationStartTime { get; private set; }

    public DateTime? RainbowBattleProcessTime { get; set; }

    public IReadOnlyList<(TimeSpan, int, TimeType)> Warnings { get; }

    public void RegisterPlayer(long id)
    {
        _lock.EnterWriteLock();
        try
        {
            _registeredPlayers.Add(id);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void UnregisterPlayer(long id)
    {
        _lock.EnterWriteLock();
        try
        {
            _registeredPlayers.Remove(id);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IEnumerable<long> RegisteredPlayers
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _registeredPlayers.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public void ClearRegisteredPlayers()
    {
        _lock.EnterWriteLock();
        try
        {
            _registeredPlayers.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void AddRainbowBattle(RainbowBattleParty rainbowBattleParty)
    {
        _rainbowBattleParties.TryAdd(rainbowBattleParty.Id, rainbowBattleParty);
    }

    public void RemoveRainbowBattle(RainbowBattleParty rainbowBattleParty)
    {
        _rainbowBattleParties.Remove(rainbowBattleParty.Id, out _);
    }

    public IEnumerable<RainbowBattleParty> RainbowBattleParties => _rainbowBattleParties.Values.ToArray();
}