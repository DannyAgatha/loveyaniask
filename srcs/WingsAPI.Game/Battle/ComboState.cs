namespace WingsEmu.Game.Battle;

public class ComboState
{
    public ComboState(long targetId)
    {
        Hit = 0;
        TargetId = targetId;
    }

    public int Hit { get; set; }
    public long TargetId { get; }
}