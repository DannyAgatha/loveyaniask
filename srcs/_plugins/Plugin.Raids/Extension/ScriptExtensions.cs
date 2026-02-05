using WingsAPI.Packets.Enums;
using WingsAPI.Scripting.Enum.Raid;

namespace Plugin.Raids.Extension;

public static class ScriptExtensions
{
    public static SRaidType ToSRaidType(this RaidType raidType) => (SRaidType)(byte)raidType;
}