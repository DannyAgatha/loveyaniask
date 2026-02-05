// NosEmu
// 


using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("Char_NEW")]
    public class CharacterCreatePacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public string Name { get; set; }

        [PacketIndex(1)]
        public byte Slot { get; set; }

        [PacketIndex(2)]
        public GenderType Gender { get; set; }

        [PacketIndex(3)]
        public HairStyleType HairStyle { get; set; }

        [PacketIndex(4)]
        public HairColorType HairColor { get; set; }

        public override string ToString() => $"Create Character Name: {Name} Slot: {Slot}";

        #endregion
    }
}