namespace WingsEmu.Game.Networking.Broadcasting;

public class ExceptSessionBroadcast : IBroadcastRule
{
    private readonly long _sessionCharacterId;

    public ExceptSessionBroadcast(IClientSession session) => _sessionCharacterId = session.PlayerEntity.Id;

    public bool Match(IClientSession session) => session.PlayerEntity.Id != _sessionCharacterId;
}