// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Relation;
using WingsEmu.Game.Relations;

namespace NosEmu.Plugins.BasicImplementations.Event.Relations;

public class RemoveRelationEventHandler : IAsyncEventProcessor<RemoveRelationEvent>
{
    private readonly IRelationService _relationService;

    public RemoveRelationEventHandler(IRelationService relationService) => _relationService = relationService;

    public async Task HandleAsync(RemoveRelationEvent e, CancellationToken cancellation)
    {
        await _relationService.RemoveRelationAsync(new RelationRemoveRequest
        {
            CharacterId = e.Sender.PlayerEntity.Id,
            RelationType = e.Type,
            TargetId = e.TargetCharacterId
        });
    }
}