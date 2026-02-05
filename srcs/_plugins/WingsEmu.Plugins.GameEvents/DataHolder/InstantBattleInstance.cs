using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.Maps;
using WingsEmu.Plugins.GameEvents.Configuration.InstantBattle;

namespace WingsEmu.Plugins.GameEvents.DataHolder
{
    public class InstantBattleInstance : IGameEventInstance
    {
        public InstantBattleInstance(IMapInstance mapInstance, InstantBattleConfiguration configuration)
        {
            MapInstance = mapInstance;
            InternalConfiguration = configuration;
            ClosingTimeWarnings = configuration.TimeLeftWarnings.ToList();
            AvailableWaves = configuration.Waves.Select(x => new InstantBattleInstanceWave(x)).ToList();
            StartDate = DateTime.UtcNow;
            DestroyDate = StartDate + configuration.ClosingTime;
        }

        public DateTime StartDate { get; }
        public bool Finished { get; set; }
        public List<TimeSpan> ClosingTimeWarnings { get; }
        public List<InstantBattleInstanceWave> AvailableWaves { get; }
        public InstantBattleConfiguration InternalConfiguration { get; }
        public GameEventType GameEventType => GameEventType.InstantBattle;
        public DateTime DestroyDate { get; set; }
        public IMapInstance MapInstance { get; }
        public IGameEventConfiguration Configuration => InternalConfiguration;
    }

    public class InstantBattleInstanceWave
    {
        public InstantBattleInstanceWave(InstantBattleWave configuration)
        {
            Configuration = configuration;
            MonsterSpawn = DateTime.UtcNow;
        }

        public InstantBattleWave Configuration { get; }

        public bool PreWaveLongWarningDone { get; set; }
        public bool PreWaveSoonWarningDone { get; set; }
        public bool StartedWave { get; set; }
        public DateTime MonsterSpawn { get; set; }
    }
}