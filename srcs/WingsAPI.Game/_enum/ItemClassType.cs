using System;

namespace WingsEmu.Game._enum;

[Flags]
public enum ItemClassType
{
    Neutral = 0,
    Adventurer = 1 << 0,
    Swordsman = 1 << 1,
    Archer = 1 << 2,
    Mage = 1 << 3,
    MartialArtist = 1 << 4
}