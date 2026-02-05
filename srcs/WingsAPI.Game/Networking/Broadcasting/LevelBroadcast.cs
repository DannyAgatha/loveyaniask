namespace WingsEmu.Game.Networking.Broadcasting;

public class LevelBroadcast : IBroadcastRule
{
    private readonly int _level;

    public LevelBroadcast(int level) => _level = level;

    public bool Match(IClientSession session) => session.PlayerEntity.Level >= _level;
}