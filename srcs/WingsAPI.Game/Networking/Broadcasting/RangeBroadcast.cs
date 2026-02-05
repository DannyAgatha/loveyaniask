using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Networking.Broadcasting;

public class RangeBroadcast : IBroadcastRule
{
    private readonly int _range;
    private readonly int _x;
    private readonly int _y;

    public RangeBroadcast(int x, int y, int range = 20)
    {
        _x = x;
        _y = y;
        _range = range;
    }

    public bool Match(IClientSession session) => session.PlayerEntity.Position.GetDistance(_x, _y) <= _range;
}