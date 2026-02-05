namespace WingsEmu.Game.Networking.Broadcasting;

public class TimeSpaceBroadcast : IBroadcastRule
{
    private readonly IClientSession _session;

    public TimeSpaceBroadcast(IClientSession session) => _session = session;

    public bool Match(IClientSession session)
    {
        if (session.PlayerEntity.Id == _session.PlayerEntity.Id)
        {
            return false;
        }

        return session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty;
    }
}