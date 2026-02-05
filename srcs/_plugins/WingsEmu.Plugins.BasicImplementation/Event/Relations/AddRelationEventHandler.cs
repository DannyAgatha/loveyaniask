// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Relation;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;

namespace NosEmu.Plugins.BasicImplementations.Event.Relations;

public class AddRelationEventHandler : IAsyncEventProcessor<AddRelationEvent>
{
    private readonly IRelationService _relationService;

    public AddRelationEventHandler(IRelationService relationService) => _relationService = relationService;

    public async Task HandleAsync(AddRelationEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        RelationAddResponse response = await _relationService.AddRelationAsync(new RelationAddRequest
        {
            CharacterId = session.PlayerEntity.Id,
            CharacterName = session.PlayerEntity.Name,
            RelationType = e.RelationType,
            TargetId = e.TargetCharacterId
        });
    }
}