using WingsEmu.DTOs.Account;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game;

public class GmCommandEvent : PlayerEvent
{
    public string Command { get; init; }
    public AuthorityType PlayerAuthority { get; init; }
    public AuthorityType CommandAuthority { get; init; }
}