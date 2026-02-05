using System;
using WingsAPI.Packets.Enums.Act4;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Managers;

public interface IAct4Manager
{
    bool FactionPointsLocked { get; }
    void AddFactionPoints(FactionType factionType, int amount);

    void ResetFactionPoints(FactionType factionType);

    void RegisterMukraju(DateTime current, IMonsterEntity mukraju, FactionType factionType);

    (DateTime deleteTime, IMonsterEntity mukraju, FactionType mukrajuFactionType) GetMukraju();

    IMonsterEntity UnregisterMukraju();

    FactionType MukrajuFaction();

    FactionType GetTriumphantFaction();

    Act4Status GetStatus();
}

public sealed record Act4Status(byte AngelPointsPercentage, byte DemonPointsPercentage, TimeSpan TimeBeforeReset, FactionType RelevantFaction, Act4FactionStateType FactionStateType,
    TimeSpan CurrentTimeBeforeMukrajuDespawn, TimeSpan TimeBeforeMukrajuDespawn, DungeonType DungeonType);