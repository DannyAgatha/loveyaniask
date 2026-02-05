using System.Collections.Generic;
using System.Linq;
using WingsAPI.Packets.Enums.WorldBoss;
using WingsEmu.Game._enum;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Configurations.WorldBoss
{
    public interface IWorldBossConfiguration
    {
        List<WorldBossModeConfiguration> GetBossesByMode(WorldBossModeType mode);
        WorldBossModeConfiguration GetBossByModeAndNpcRun(WorldBossModeType mode, NpcRunType npcRunType);
        WorldBossModeConfiguration GetBossByVnum(int bossVnum);
    }

    public class WorldBossConfiguration : IWorldBossConfiguration
    {
        private readonly List<WorldBossModeConfiguration> _bosses;

        public WorldBossConfiguration(IEnumerable<WorldBossModeConfiguration> bosses)
        {
            _bosses = bosses?.ToList() ?? [];
        }

        public List<WorldBossModeConfiguration> GetBossesByMode(WorldBossModeType mode)
        {
            return _bosses.Where(b => b.Mode == mode).ToList();
        }

        public WorldBossModeConfiguration GetBossByModeAndNpcRun(WorldBossModeType mode, NpcRunType npcRunType)
        {
            return _bosses.FirstOrDefault(b => b.Mode == mode && b.NpcRunType == npcRunType);
        }
        
        public WorldBossModeConfiguration GetBossByVnum(int bossVnum)
        {
            return _bosses.FirstOrDefault(b => b.BossVnum == bossVnum);
        }
    }

    public class WorldBossModeConfiguration
    {
        public WorldBossModeType Mode { get; set; }
        public NpcRunType NpcRunType { get; set; }
        public int MinLevel { get; set; }
        public int MinHeroLevel { get; set; }
        public int BossVnum { get; set; }
        public short BossPositionX { get; set; }
        public short BossPositionY { get; set; }
        public byte BossDirection { get; set; }
        public int MapVnum { get; set; }
        public short DestinationX { get; set; }
        public short DestinationY { get; set; }
        public byte PlayerDirection { get; set; }
        public WorldBossRewardModeType RewardMode { get; set; }
        public double BaseProbability { get; set; }
        public int TimeLimitMinutes { get; set; }
        public int BoostRaffleChance { get; set; }   
        public int BoostRaffleMin { get; set; }
        public int BoostRaffleMax { get; set; }
    }
}
