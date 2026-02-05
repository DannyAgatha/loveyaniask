using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Monster.Event;

public class MonsterSummonEvent : IAsyncEvent
{
    public MonsterSummonEvent(IMapInstance map, IEnumerable<ToSummon> monsters, IBattleEntity summoner = null,
        bool getSummonerLevel = true, bool showEffect = false, short? scaledWithPlayerAmount = null)
    {
        Map = map;
        Monsters = monsters;
        Summoner = summoner;
        GetSummonerLevel = getSummonerLevel;
        ShowEffect = showEffect;
        ScaledWithPlayerAmount = scaledWithPlayerAmount;
    }

    public IMapInstance Map { get; }
    public IEnumerable<ToSummon> Monsters { get; }
    public IBattleEntity Summoner { get; }
    public bool GetSummonerLevel { get; }
    public bool ShowEffect { get; }
    public short? ScaledWithPlayerAmount { get; }
    public Guid? NpcId { get; init; }
}