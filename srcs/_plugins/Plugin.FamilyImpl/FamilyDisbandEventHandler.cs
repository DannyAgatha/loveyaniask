using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Families;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyDisbandEventHandler : IAsyncEventProcessor<FamilyDisbandEvent>
    {
        private readonly IFamilyService _familyService;

        public FamilyDisbandEventHandler(IFamilyService familyService) => _familyService = familyService;

        public async Task HandleAsync(FamilyDisbandEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IFamily family = session.PlayerEntity.Family;
            if (family == null || session.PlayerEntity.GetFamilyAuthority() != FamilyAuthority.Head)
            {
                return;
            }

            await _familyService.DisbandFamilyAsync(new FamilyDisbandRequest
            {
                FamilyId = family.Id
            });

            await session.EmitEventAsync(new FamilyDisbandedEvent
            {
                FamilyId = family.Id
            });
        }
    }
}