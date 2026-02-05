// NosEmu
// 


using System.Collections.Generic;
using ProtoBuf;

namespace WingsEmu.DTOs.Quests;

[ProtoContract]
public class CharacterQuestDto
{
    [ProtoMember(1)]
    public int QuestId { get; set; }

    [ProtoMember(2)]
    public QuestSlotType SlotType { get; set; }

    [ProtoMember(3)]
    public Dictionary<int, CharacterQuestObjectiveDto> ObjectiveAmount { get; set; }
}