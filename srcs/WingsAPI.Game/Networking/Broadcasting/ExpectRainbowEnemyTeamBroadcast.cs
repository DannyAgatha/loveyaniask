namespace WingsEmu.Game.Networking.Broadcasting;

public class RainbowTeamBroadcast : IBroadcastRule
{
    private readonly IClientSession _session;

    public RainbowTeamBroadcast(IClientSession session) => _session = session;

    public bool Match(IClientSession session)
    {
        if (!session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
        {
            return true;
        }

        if (!_session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
        {
            return true;
        }

        return session.PlayerEntity.RainbowBattleComponent.Team == _session.PlayerEntity.RainbowBattleComponent.Team;
    }
}