using ProtoBuf;

namespace WingsEmu.DTOs.Quests;

[ProtoContract]
public class CharacterQuestObjectiveDto
{
    [ProtoMember(1)]
    public int CurrentAmount { get; set; }

    [ProtoMember(2)]
    public int RequiredAmount { get; set; }
}