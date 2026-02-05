using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act5;

public class Act5OpenNpcRunEvent : PlayerEvent
{
    public NpcRunType NpcRunType { get; init; }
    public bool IsConfirm { get; init; }
}