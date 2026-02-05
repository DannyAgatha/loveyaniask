using WingsEmu.Game.Alzanor.Configurations;

namespace WingsEmu.Game.Alzanor;

public interface IAlzanorComponent
{
    int Kills { get; set; }
    int Deaths { get; set; }
    bool IsInAlzanorEvent { get; }
    AlzanorParty AlzanorParty { get; }
    AlzanorTeamType Team { get; }
    void SetAlzanorEvent(AlzanorParty bufferBattleParty, AlzanorTeamType team);
    void RemoveAlzanorEvent();
}

public class AlzanorComponent : IAlzanorComponent
{
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public bool IsInAlzanorEvent => AlzanorParty != null;
    public AlzanorParty AlzanorParty { get; private set; }
    public AlzanorTeamType Team { get; private set; }
    public void SetAlzanorEvent(AlzanorParty alzanorParty, AlzanorTeamType team)
    {
        AlzanorParty = alzanorParty;
        Team = team;
        Kills = 0;
        Deaths = 0;
    }

    public void RemoveAlzanorEvent()
    {
        AlzanorParty = null;
        Kills = 0;
        Deaths = 0;
    }
}