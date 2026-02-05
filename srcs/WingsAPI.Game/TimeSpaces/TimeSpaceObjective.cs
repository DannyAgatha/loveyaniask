namespace WingsEmu.Game.TimeSpaces;

public class TimeSpaceObjective
{
    public bool KillAllMonsters { get; set; }
    public bool GoToExit { get; set; }
    public bool ProtectNPC { get; set; }

    public short? KillMonsterVnum { get; set; }
    public short? KillMonsterAmount { get; set; }
    public short KilledMonsterAmount { get; set; }

    public short? CollectItemVnum { get; set; }
    public short? CollectItemAmount { get; set; }
    public short CollectedItemAmount { get; set; }

    public byte? Conversation { get; set; }
    public byte ConversationsHad { get; set; }

    public short? InteractObjectsVnum { get; set; }
    public short? InteractObjectsAmount { get; set; }
    public short InteractedObjectsAmount { get; set; }
}