using System.Threading;
using System.Threading.Tasks;
using FamilyServer.Services;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Families;

namespace FamilyServer.Consumers
{
    public class FamilyMissionsResetMessageConsumer : IMessageConsumer<FamilyMissionsResetMessage>
    {
        private readonly FamilyService _familyService;

        public FamilyMissionsResetMessageConsumer(FamilyService familyService) => _familyService = familyService;

        public async Task HandleAsync(FamilyMissionsResetMessage notification, CancellationToken token)
        {
            await _familyService.ResetFamilyMissions();
        }
    }
}