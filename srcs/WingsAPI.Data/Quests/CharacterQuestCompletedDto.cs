using System;

namespace WingsEmu.DTOs.Quests;

public class CharacterQuestCompletedDto
{
    public int QuestId { get; set; }
    public DateTime CompletionDate { get; set; }
}