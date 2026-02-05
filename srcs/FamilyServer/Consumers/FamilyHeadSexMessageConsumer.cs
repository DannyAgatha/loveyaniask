using System.Threading;
using System.Threading.Tasks;
using FamilyServer.Managers;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;
using WingsEmu.Packets.Enums.Character;

namespace FamilyServer.Consumers
{
    public class FamilyHeadSexMessageConsumer : IMessageConsumer<FamilyHeadSexMessage>
    {
        private readonly FamilyManager _familyManager;
        private readonly IMessagePublisher<FamilyUpdateMessage> _messagePublisher;

        public FamilyHeadSexMessageConsumer(FamilyManager familyManager, IMessagePublisher<FamilyUpdateMessage> messagePublisher)
        {
            _familyManager = familyManager;
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(FamilyHeadSexMessage notification, CancellationToken token)
        {
            long familyId = notification.FamilyId;
            GenderType genderType = notification.NewGenderType;

            FamilyDTO familyDto = await _familyManager.GetFamilyByFamilyIdAsync(familyId);
            if (familyDto == null)
            {
                return;
            }

            if (familyDto.HeadGender == genderType)
            {
                return;
            }

            familyDto.HeadGender = genderType;
            await _familyManager.SaveFamilyAsync(familyDto);

            await _messagePublisher.PublishAsync(new FamilyUpdateMessage
            {
                ChangedInfoFamilyUpdate = ChangedInfoFamilyUpdate.HeadSex,
                Families = new[] { familyDto }
            });
        }
    }
}