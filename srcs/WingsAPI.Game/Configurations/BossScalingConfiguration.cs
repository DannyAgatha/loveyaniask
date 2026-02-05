using System.Collections.Generic;
using WingsAPI.Packets.Enums;

namespace WingsEmu.Game.Configurations
{
    public interface IBossScalingConfiguration
    {
        int GetBossHpPercentageByPlayersAndRaidType(RaidType raidType, int players);
        ModeType GetModeTypeByRaidTypeAndPlayers(RaidType raidType, int players);
    }

    public class BossScalingConfiguration : IBossScalingConfiguration
    {
        private readonly BossScalingFileConfiguration _bossScalingConfig;
        
        public BossScalingConfiguration(BossScalingFileConfiguration config) => _bossScalingConfig = config;

        public int GetBossHpPercentageByPlayersAndRaidType(RaidType raidType, int players)
        {
            BossScaling bossScaling = _bossScalingConfig.Find(config => config.RaidType == raidType);
            SubBossScaling subBossScaling = bossScaling?.BossScalings.Find(subConfig => subConfig.Players == players);
            return subBossScaling?.BossHpPercentage ?? 100; // Return 100% hp, if value not found.
        }
        
        public ModeType GetModeTypeByRaidTypeAndPlayers(RaidType raidType, int players)
        {
            BossScaling bossScaling = _bossScalingConfig.Find(config => config.RaidType == raidType);
            SubBossScaling subBossScaling = bossScaling?.BossScalings.Find(subConfig => subConfig.Players == players);
            return subBossScaling?.ModeType ?? ModeType.NORMAL; // Return ModeType: NORMAL, if value not found.
        }
    }
    
    public class BossScalingFileConfiguration : List<BossScaling>
    {
    }
    
    public class BossScaling
    {
        public RaidType RaidType { get; set; }
        public List<SubBossScaling> BossScalings { get; set; }
    }

    public class SubBossScaling
    {
        public int Players { get; set; }
        public int BossHpPercentage { get; set; }
        public ModeType ModeType { get; set; }
    }
}