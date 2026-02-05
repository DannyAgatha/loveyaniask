using System.Collections.Generic;
using WingsAPI.Packets.Enums;

namespace WingsEmu.Game.Configurations
{
    public interface IRaidModeConfiguration
    {
        RaidMode GetRaidType(RaidType raidType);
        RaidModeType GetModeType(RaidType raidType, ModeType modeType);
    }
    
    public class RaidModeConfiguration : IRaidModeConfiguration
    {
        private readonly RaidModeFileConfiguration _raidModes;

        public RaidModeConfiguration(RaidModeFileConfiguration raidModes) => _raidModes = raidModes;

        public RaidMode GetRaidType(RaidType raidType)
        {
            return _raidModes.Find(mode => mode.RaidType == raidType);
        }

        public RaidModeType GetModeType(RaidType raidType, ModeType modeType)
        {
            RaidMode raidMode = GetRaidType(raidType);
            return raidMode?.Modes.Find(mode => mode.ModeType == modeType);
        }
    }
    
    public class RaidModeFileConfiguration : List<RaidMode>
    {
    }

    public class RaidMode
    {
        public RaidType RaidType { get; set; }
        public List<RaidModeType> Modes { get; set; }
    }

    public class RaidModeType
    {
        public ModeType ModeType { get; set; }
        public RewardsMultipliers RewardsMultiplier { get; set; }
    }

    public class RewardsMultipliers
    {
        public int Reputation { get; set; }
        public int ItemsAmount { get; set; }
        public int Gold { get; set; }
        public int Rarity { get; set; }
        public int Fxp { get; set; }
    }
}