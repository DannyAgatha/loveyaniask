namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("DAC")]
    public class CrossServerEntrypointPacket : ClientPacket
    {
        [PacketIndex(0)]
        public string AccountName { get; set; }

        [PacketIndex(1)]
        public byte CharacterSlot { get; set; }
    }
}