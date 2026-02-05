using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Enum.TimeSpace;

namespace WingsAPI.Scripting.Object.Timespace
{
    [ScriptObject]
    public class STimeSpaceTask
    {
        public STimeSpaceTaskType TimeSpaceTaskType { get; set; }
        public string GameDialogKey { get; set; }
        public short? DurationInSeconds { get; set; }

        public int? StartDialog { get; set; }
        public bool DialogStartTask { get; set; }
        public int? EndDialog { get; set; }
        public string StartDialogShout { get; set; }
        public string EndDialogShout { get; set; }

        public bool StartDialogIsObjective { get; set; }
        public bool EndDialogIsObjective { get; set; }
    }
}