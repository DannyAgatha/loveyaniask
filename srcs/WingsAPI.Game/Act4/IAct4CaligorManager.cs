using System;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act4
{
    public interface IAct4CaligorManager
    {
        public DateTime CaligorEnd { get; set; }
        public bool CaligorActive { get; set; }
        public int AngelDamage { get; set; }
        public int DemonDamage { get; set; }
        public int AngelCount { get; set; }
        public int DemonCount { get; set; }
        public bool FernonMapsActive { get; }
        public IMapInstance FernonMap { get; }
        public FactionType GetInitialFaction(IPlayerEntity player);
        public bool HasLeftCaligor(IPlayerEntity player);
        public void MarkPlayerAsLeftCaligor(IPlayerEntity player);
        public void TeleportPlayerToCaligorCamp(IPlayerEntity player);
        public void EndCaligorInstance(bool caligorGotKilled);
        public void RefreshCaligorInstance();
        public void InitializeAndStartCaligorInstance();
        public void UpdateFactionCount(FactionType faction);
        public void DecreaseFactionCount(FactionType faction);
    }
}