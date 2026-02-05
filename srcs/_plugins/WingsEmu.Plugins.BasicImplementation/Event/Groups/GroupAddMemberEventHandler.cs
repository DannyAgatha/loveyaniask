using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Groups;

public class AddMemberToGroupEventHandler : IAsyncEventProcessor<GroupAddMemberEvent>
{
    private readonly IGroupManager _group;

    public AddMemberToGroupEventHandler(IGroupManager group) => _group = group;

    public async Task HandleAsync(GroupAddMemberEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (!session.PlayerEntity.IsInGroup())
        {
            return;
        }

        PlayerGroup playerGroup = session.PlayerEntity.GetGroup();
        _group.AddMemberGroup(playerGroup, e.NewMember);
    }
}