using System;
using System.Collections.Generic;
using WingsEmu.Game.Entities;
using WingsEmu.Game.TimeSpaces.Enums;

namespace WingsEmu.Game.TimeSpaces;

public class TimeSpaceTask
{
    public TimeSpaceTaskType TaskType { get; set; }
    public TimeSpan? Time { get; set; }
    public string GameDialogKey { get; set; }
    public bool IsActivated { get; set; }
    public bool IsFinished { get; set; }
    public DateTime TaskStart { get; set; }
    public DateTime? TimeLeft { get; set; }
    public List<(int?, IMonsterEntity)> MonstersAfterTaskStart { get; } = new();
    public int? StartDialog { get; set; }
    public bool StartDialogIsObjective { get; set; }
    public bool DialogStartTask { get; set; }
    public int? EndDialog { get; set; }
    public bool EndDialogIsObjective { get; set; }
    public string StartDialogShout { get; set; }
    public string EndDialogShout { get; set; }
}