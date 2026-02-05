using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Groups.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Groups;

public class JoinToGroupEventHandler : IAsyncEventProcessor<JoinToGroupEvent>
{
    private readonly IGroupManager _group;
    public JoinToGroupEventHandler(IGroupManager group) => _group = group;

    public async Task HandleAsync(JoinToGroupEvent e, CancellationToken cancellation)
    {
        IPlayerEntity playerEntity = e.Sender.PlayerEntity;
        _group.JoinGroup(e.PlayerGroup, playerEntity);
        playerEntity.CheckWeedingBuff = DateTime.UtcNow;
    }
}