using System.Collections.Generic;

namespace WingsEmu.Game.Configurations;

public class QuestTeleportDialogConfiguration : List<QuestTeleportDialogInfo>
{
}

public class QuestTeleportDialogInfo
{
    public int RunId { get; set; }
    public int MapId { get; set; }
    public bool AskForTeleport { get; set; }
    public short PositionX { get; set; }
    public short PositionY { get; set; }
}