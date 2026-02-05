using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core;
using WingsEmu.Game.Characters;
using WingsEmu.Game.GameEvent.Configuration;

namespace WingsEmu.Plugins.GameEvents.Configuration.InstantBattle
{
    public class GlobalInstantBattleConfiguration : IGlobalInstantBattleConfiguration
    {
        public List<InstantBattleConfiguration> Configurations { get; set; }

        public InstantBattleConfiguration GetInternalConfiguration(IPlayerEntity character)
        {
            InstantBattleConfiguration config;
            if (character.HeroLevel > 0)
            {
                config = GetInternalConfiguration(character.HeroLevel, true);
            }
            else
            {
                config = GetInternalConfiguration(character.Level, false);
            }

            return config;
        }

        public IGameEventConfiguration GetConfiguration(IPlayerEntity character) => GetInternalConfiguration(character);

        public uint GetRegistrationCost(IPlayerEntity character) => 0;

        public IEnumerable<Range<byte>> GetNormalRanges()
        {
            return Configurations.Select(x => x.Requirements.Level).Where(x => x != null);
        }

        public IEnumerable<Range<byte>> GetHeroicRanges()
        {
            return Configurations.Select(x => x.Requirements.HeroicLevel).Where(x => x != null);
        }

        public IGameEventConfiguration GetConfiguration(byte level, bool heroic) => GetInternalConfiguration(level, heroic);

        private InstantBattleConfiguration GetInternalConfiguration(byte level, bool heroic)
        {
            return heroic
                ? Configurations.Find(x => x.Requirements.HeroicLevel != null && level >= x.Requirements.HeroicLevel.Minimum && level <= x.Requirements.HeroicLevel.Maximum)
                : Configurations.Find(x => x.Requirements.Level != null && level >= x.Requirements.Level.Minimum && level <= x.Requirements.Level.Maximum);
        }
    }
}