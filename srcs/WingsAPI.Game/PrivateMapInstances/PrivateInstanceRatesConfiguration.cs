using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.PrivateMapInstances.Events;

namespace WingsEmu.Game.PrivateMapInstances;

public sealed class PrivateInstanceRates
{
    public double DropItem { get; init; }
    public double DropGold { get; init; }
    public double ExpCharacter { get; init; }
    public double ExpHero { get; init; }
    public double ExpFairy { get; init; }
    public double ExpJob { get; init; }
    public double ExpSp { get; init; }
    public double ExpMate { get; init; }
    public int MonsterAdditionalSpeed { get; init; }
    public double MonsterRespawnReducer { get; init; }
    public long InstanceCostGold { get; init; }
}

public sealed class PrivateInstanceRatesConfiguration
{
    public Dictionary<string, PrivateInstanceRates> Premium { get; init; } = new();
    
    public Dictionary<string, PrivateInstanceRates> Hardcore { get; init; } = new();

    public PrivateInstanceRates GetRates(PrivateMapInstanceType type, bool isPremium, bool isHardcore)
    {
        string key = type.ToString().ToUpperInvariant();

        if (isHardcore)
        {
            if (Hardcore.TryGetValue(key, out PrivateInstanceRates hardcoreRates))
            {
                return hardcoreRates;
            }
            throw new KeyNotFoundException($"No Hardcore config found for PrivateMapInstanceType: {type}");
        }

        if (isPremium)
        {
            if (Premium.TryGetValue(key, out PrivateInstanceRates premiumRates))
            {
                return premiumRates;
            }
            throw new KeyNotFoundException($"No Premium config found for PrivateMapInstanceType: {type}");
        }

        throw new InvalidOperationException("Neither premium nor hardcore flag specified when requesting PrivateInstanceRates.");
    }
}


