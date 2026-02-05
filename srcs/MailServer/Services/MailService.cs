using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailServer.Managers;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication;
using WingsAPI.Communication.Mail;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Mails;
using WingsEmu.Plugins.DistributedGameEvents.Mails;

namespace MailServer.Services
{
    public class MailService : IMailService
    {
        private readonly MailManager _mailManager;
        private readonly IMessagePublisher<MailReceivedMessage> _messagePublisher;

        public MailService(IMessagePublisher<MailReceivedMessage> messagePublisher, MailManager mailManager)
        {
            _messagePublisher = messagePublisher;
            _mailManager = mailManager;
        }

        public async ValueTask<CreateMailResponse> CreateMailAsync(CreateMailRequest request)
        {
            string senderName = request.SenderName;
            long receiverId = request.ReceiverId;
            MailGiftType mailGiftType = request.MailGiftType;
            ItemInstanceDTO itemInstance = request.ItemInstance;

            var newMail = new CharacterMailDto
            {
                Date = DateTime.UtcNow,
                SenderName = senderName,
                ReceiverId = receiverId,
                MailGiftType = mailGiftType,
                ItemInstance = itemInstance
            };

            _mailManager.AddMail((newMail, false));

            return new CreateMailResponse
            {
                Status = RpcResponseType.SUCCESS,
                Mail = newMail
            };
        }

        public async Task<CreateMailBatchResponse> CreateMailBatchAsync(CreateMailBatchRequest request)
        {
            if (request.Mails == null)
            {
                return new CreateMailBatchResponse
                {
                    Status = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            if (request.Bufferized)
            {
                _mailManager.AddMails(request.Mails.Select(m => (m, false)));
            }
            else
            {
                await _mailManager.AddMailsInstantly(request.Mails);
            }

            return new CreateMailBatchResponse
            {
                Status = RpcResponseType.SUCCESS,
                Mail = request.Mails
            };
        }

        public async ValueTask<BasicRpcResponse> RemoveMailAsync(RemoveMailRequest request)
        {
            IReadOnlyDictionary<long, CharacterMailDto> dictionary = await _mailManager.GetCharacterMailsDictionary(request.CharacterId);
            if (!dictionary.TryGetValue(request.MailId, out CharacterMailDto dto))
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            await _mailManager.RemoveMail(request.CharacterId, dto);

            return new BasicRpcResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };
        }

        public async ValueTask<GetMailsResponse> GetMailsByCharacterId(GetMailsRequest request)
        {
            long characterId = request.CharacterId;

            return new GetMailsResponse
            {
                CharacterMailsDto = await _mailManager.GetCharacterMails(characterId)
            };
        }
    }
}