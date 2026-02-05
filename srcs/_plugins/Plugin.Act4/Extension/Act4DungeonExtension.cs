using WingsEmu.Game.Act4.Entities;
using WingsEmu.Game.Networking;

namespace Plugin.Act4.Extension;

public static class Act4DungeonExtension
{
    public static string HatusHeadStatePacket(short whichHeadsBitFlag, HatusHeads heads) =>
        $"bc 1 {(byte)heads.HeadsState} {whichHeadsBitFlag} {heads.DragonHeads.BluePositionX} {heads.DragonHeads.RedPositionX} {heads.DragonHeads.GreenPositionX}";

    public static int GetDungeonReputationRequirement(this IClientSession session, int multiplier) => session.PlayerEntity.Level * multiplier;
}