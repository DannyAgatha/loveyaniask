using System.Threading;
using System.Threading.Tasks;
using FamilyServer.Managers;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;

namespace FamilyServer.Consumers
{
    public class FamilyNoticeMessageConsumer : IMessageConsumer<FamilyNoticeMessage>
    {
        private readonly FamilyManager _familyManager;
        private readonly IMessagePublisher<FamilyUpdateMessage> _messagePublisher;

        public FamilyNoticeMessageConsumer(FamilyManager familyManager, IMessagePublisher<FamilyUpdateMessage> messagePublisher)
        {
            _familyManager = familyManager;
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(FamilyNoticeMessage notification, CancellationToken token)
        {
            long familyId = notification.FamilyId;
            string message = notification.Message;

            FamilyDTO familyDto = await _familyManager.GetFamilyByFamilyIdAsync(familyId);
            if (familyDto == null)
            {
                return;
            }

            familyDto.Message = message;
            await _familyManager.SaveFamilyAsync(familyDto);

            await _messagePublisher.PublishAsync(new FamilyUpdateMessage
            {
                ChangedInfoFamilyUpdate = ChangedInfoFamilyUpdate.Notice,
                Families = new[] { familyDto }
            });
        }
    }
}