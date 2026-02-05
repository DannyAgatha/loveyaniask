// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("rl")]
    public class RlPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public short Type { get; set; }

        [PacketIndex(1)]
        public string CharacterName { get; set; }

        #endregion
    }
}