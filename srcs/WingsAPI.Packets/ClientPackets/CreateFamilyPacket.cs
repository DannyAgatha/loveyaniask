// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("glmk")]
    public class CreateFamilyPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public string FamilyName { get; set; }

        #endregion
    }
}