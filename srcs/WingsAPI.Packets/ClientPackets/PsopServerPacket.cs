// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("ps_op")]
    public class PsopServerPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte PetSlot { get; set; }

        [PacketIndex(1)]
        public byte SkillSlot { get; set; }

        [PacketIndex(2)]
        public byte Option { get; set; }

        #endregion
    }
}