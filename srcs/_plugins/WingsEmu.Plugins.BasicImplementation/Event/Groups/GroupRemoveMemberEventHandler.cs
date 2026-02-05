using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Groups;

public class RemoveMemberFromGroupEventHandler : IAsyncEventProcessor<RemoveMemberFromGroupEvent>
{
    private readonly IGroupManager _group;

    public RemoveMemberFromGroupEventHandler(IGroupManager group) => _group = group;

    public async Task HandleAsync(RemoveMemberFromGroupEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (!session.PlayerEntity.IsInGroup())
        {
            return;
        }

        PlayerGroup playerGroup = session.PlayerEntity.GetGroup();
        _group.RemoveMemberGroup(playerGroup, e.MemberToRemove);
    }
}