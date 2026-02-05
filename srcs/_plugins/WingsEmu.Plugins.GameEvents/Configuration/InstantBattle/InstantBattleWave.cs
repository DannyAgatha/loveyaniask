using System;
using System.Collections.Generic;

namespace WingsEmu.Plugins.GameEvents.Configuration.InstantBattle
{
    public class InstantBattleWave
    {
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public string TitleKey { get; set; }
        public int WaveLevel { get; set; }
        public List<InstantBattleMonster> Monsters { get; set; }
        public List<InstantBattleDrop> Drops { get; set; }
    }

    public class InstantBattleMonster
    {
        public short MonsterVnum { get; set; }
        public int Amount { get; set; }
    }

    public class InstantBattleDrop
    {
        public short ItemVnum { get; set; }
        public int AmountPerBunch { get; set; }
    }
}