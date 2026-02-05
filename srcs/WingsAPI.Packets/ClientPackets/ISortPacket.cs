using WingsEmu.Packets;

namespace WingsAPI.Packets.ClientPackets
{
    [PacketHeader("isort")]
    public class ISortPacket : ClientPacket
    {
        [PacketIndex(0)]
        public byte InventoryType { get; set; }

        [PacketIndex(1)]
        public bool Confirm { get; set; }
    }
}