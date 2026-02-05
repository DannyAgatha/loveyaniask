namespace WingsEmu.Game.Skills;

public interface IScoutComponent
{
    public ScoutStateType ScoutStateType { get; }
    public void ChangeScoutState(ScoutStateType stateType);
}

public class ScoutComponent : IScoutComponent
{
    public ScoutComponent() => ScoutStateType = ScoutStateType.None;

    public ScoutStateType ScoutStateType { get; private set; }

    public void ChangeScoutState(ScoutStateType stateType)
    {
        if (ScoutStateType == stateType)
        {
            return;
        }

        ScoutStateType = stateType;
    }
}

public enum ScoutStateType : byte
{
    None = 0,
    FirstState = 1,
    SecondState = 2
}