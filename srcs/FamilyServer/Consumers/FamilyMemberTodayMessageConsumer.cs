using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FamilyServer.Managers;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;

namespace FamilyServer.Consumers
{
    public class FamilyMemberTodayMessageConsumer : IMessageConsumer<FamilyMemberTodayMessage>
    {
        private readonly FamilyMembershipManager _familyMembershipManager;
        private readonly IMessagePublisher<FamilyMemberUpdateMessage> _messagePublisher;

        public FamilyMemberTodayMessageConsumer(FamilyMembershipManager familyMembershipManager, IMessagePublisher<FamilyMemberUpdateMessage> messagePublisher)
        {
            _familyMembershipManager = familyMembershipManager;
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(FamilyMemberTodayMessage notification, CancellationToken token)
        {
            FamilyMembershipDto familyMember = await _familyMembershipManager.GetFamilyMembershipByCharacterIdAsync(notification.CharacterId);
            if (familyMember == null)
            {
                return;
            }

            familyMember.DailyMessage = notification.Message;

            await _familyMembershipManager.SaveFamilyMembershipAsync(familyMember);
            await _messagePublisher.PublishAsync(new FamilyMemberUpdateMessage
            {
                ChangedInfoMemberUpdate = ChangedInfoMemberUpdate.DailyMessage,
                UpdatedMembers = new List<FamilyMembershipDto>
                {
                    familyMember
                }
            });
        }
    }
}