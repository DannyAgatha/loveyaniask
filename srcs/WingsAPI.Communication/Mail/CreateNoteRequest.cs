using ProtoBuf;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsAPI.Communication.Mail
{
    [ProtoContract]
    public class CreateNoteRequest
    {
        [ProtoMember(1)]
        public long SenderId { get; set; }

        [ProtoMember(2)]
        public string SenderName { get; set; }

        [ProtoMember(3)]
        public long ReceiverId { get; set; }

        [ProtoMember(4)]
        public string ReceiverName { get; set; }

        [ProtoMember(5)]
        public string Title { get; set; }

        [ProtoMember(6)]
        public string Message { get; set; }

        [ProtoMember(7)]
        public string EquipmentPackets { get; set; }

        [ProtoMember(8)]
        public GenderType SenderGender { get; set; }

        [ProtoMember(9)]
        public ClassType SenderClass { get; set; }

        [ProtoMember(10)]
        public HairColorType SenderHairColor { get; set; }

        [ProtoMember(11)]
        public HairStyleType SenderHairStyle { get; set; }
    }
}