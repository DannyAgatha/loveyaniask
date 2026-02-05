using WingsAPI.Packets.Enums.PartnerFusion;

namespace WingsEmu.Packets.ServerPackets;

[PacketHeader("ptsp_data")]

public class PtspDataPacket : ServerPacket
{
    [PacketIndex(0)]
    public PtspDataType DataType { get; set; }
    
    [PacketIndex(1)]
    public long PspLevelOrGold { get; set; }
    
    [PacketIndex(2)]
    public int PercentageOrItemVNum { get; set; }
    
    [PacketIndex(3)]
    public int PspNewLevelOrAmountOrSlot { get; set; }
    
    [PacketIndex(4)]
    public int NewPercentage { get; set; }
}