using ProtoBuf;

namespace WingsAPI.Data.Character;

[ProtoContract]
public class TrainerQuestDto
{
    [ProtoMember(1)]
    public byte MissionType { get; set; }

    [ProtoMember(2)]
    public short Achievement { get; set; }
}