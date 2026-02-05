// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("Char_REN")]
    public class CharacterRenamePacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Slot { get; set; }

        [PacketIndex(1)]
        public string Name { get; set; }

        #endregion
    }
}