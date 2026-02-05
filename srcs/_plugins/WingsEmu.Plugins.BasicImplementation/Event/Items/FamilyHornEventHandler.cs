using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Families;
using WingsEmu.Game.Items.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class FamilyHornEventHandler : IAsyncEventProcessor<FamilyHornEvent>
{
    private readonly ISessionManager _sessionManager;

    public FamilyHornEventHandler(ISessionManager sessionManager) => _sessionManager = sessionManager;

    public async Task HandleAsync(FamilyHornEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        List<FamilyMembership> familyMembers = session.PlayerEntity.Family.Members;

        foreach (IClientSession otherSession in from familyMember in familyMembers where familyMember.CharacterId != session.PlayerEntity.Id select _sessionManager.GetSessionByCharacterId(familyMember.CharacterId) into otherSession where otherSession != null where otherSession.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) where otherSession.PlayerEntity.MapInstance.Id != session.PlayerEntity.MapInstance.Id select otherSession)
        {
            await session.EmitEventAsync(new InvitationEvent(otherSession.PlayerEntity.Id, InvitationType.FamilySummoningHorn));
        }
    }
}