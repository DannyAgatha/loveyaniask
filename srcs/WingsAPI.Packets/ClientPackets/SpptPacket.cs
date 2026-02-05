using WingsAPI.Packets.Enums.PartnerFusion;
using WingsEmu.Packets;

namespace WingsAPI.Packets.ClientPackets;

[PacketHeader("sppt")]

public class SpptPacket : ClientPacket
{
    [PacketIndex(0)]
    public PartnerFusionSlotType SlotType { get; set; }
    
    [PacketIndex(1)]
    public short Slot { get; set; }
    
    [PacketIndex(2)]
    public short? Slot2 { get; set; }
}