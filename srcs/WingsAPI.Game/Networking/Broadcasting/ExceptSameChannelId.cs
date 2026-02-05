using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Managers;

public class ExceptSameChannelId : IBroadcastRule
{
    private readonly long _channelId;
    public ExceptSameChannelId(int senderId) => _channelId = senderId;

    public bool Match(IClientSession session) => StaticServerManager.Instance.ChannelId != _channelId;
}