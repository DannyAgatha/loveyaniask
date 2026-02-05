using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.Maps;

namespace WingsEmu.Plugins.GameEvents.Configuration.InstantBattle
{
    public class InstantBattleConfiguration : IGameEventConfiguration
    {
        private List<TimeSpan> _timeLeftWarnings;

        private List<InstantBattleWave> _waves;

        public TimeSpan ClosingTime { get; set; }
        public TimeSpan PreWaveLongWarningTime { get; set; }
        public TimeSpan PreWaveSoonWarningTime { get; set; }
        public short ReturnPortalX { get; set; }
        public short ReturnPortalY { get; set; }
        public InstantBattleRequirement Requirements { get; set; }
        public InstantBattleReward Reward { get; set; }

        public List<TimeSpan> TimeLeftWarnings
        {
            get => _timeLeftWarnings;
            set => _timeLeftWarnings = value.OrderBy(x => x).ToList();
        }

        public List<InstantBattleWave> Waves
        {
            get => _waves;
            set => _waves = value.OrderBy(x => x.TimeStart).ToList();
        }

        public GameEventType GameEventType => GameEventType.InstantBattle;

        public short MapId { get; set; } // 2004
        public MapInstanceType MapInstanceType => MapInstanceType.EventGameInstance;
    }
}