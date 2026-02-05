using System.Collections.Generic;
using WingsAPI.Packets.Enums;

namespace Plugin.Raids.Configs;

/// <summary>
///     By RaidType
/// </summary>
public class RaidStartFileConfiguration : Dictionary<RaidType, RaidStartConfiguration>
{
}