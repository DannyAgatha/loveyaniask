using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game._NpcDialog.Event;

public class NpcDialogEvent : PlayerEvent
{
    public NpcRunType NpcRunType { get; set; }

    public int Argument { get; set; }

    public VisualType VisualType { get; set; }

    public long NpcId { get; set; }

    public byte? Confirmation { get; set; }
}