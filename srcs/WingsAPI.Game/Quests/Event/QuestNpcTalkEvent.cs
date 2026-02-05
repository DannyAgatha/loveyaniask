using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Quests.Event;

public class QuestNpcTalkEvent : PlayerEvent
{
    public QuestNpcTalkEvent(CharacterQuest characterQuest, INpcEntity npcEntity, bool isByBlueAlertNrun = false)
    {
        CharacterQuest = characterQuest;
        NpcEntity = npcEntity;
        IsByBlueAlertNrun = isByBlueAlertNrun;
    }

    public CharacterQuest CharacterQuest { get; }
    public INpcEntity NpcEntity { get; }
    public bool IsByBlueAlertNrun { get; }
}