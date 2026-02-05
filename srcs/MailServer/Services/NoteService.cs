using System;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication;
using WingsAPI.Communication.Mail;
using WingsEmu.DTOs.Mails;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace MailServer.Services
{
    public class NoteService : INoteService
    {
        private readonly ICharacterNoteDao _characterNoteDao;
        private readonly IMessagePublisher<NoteReceivedMessage> _messagePublisher;

        public NoteService(ICharacterNoteDao characterNoteDao, IMessagePublisher<NoteReceivedMessage> messagePublisher)
        {
            _characterNoteDao = characterNoteDao;
            _messagePublisher = messagePublisher;
        }

        public async ValueTask<CreateNoteResponse> CreateNoteAsync(CreateNoteRequest request)
        {
            long senderId = request.SenderId;
            string senderName = request.SenderName;
            long receiverId = request.ReceiverId;
            string receiverName = request.ReceiverName;
            string title = request.Title;
            string message = request.Message;
            string equipmentPackets = request.EquipmentPackets;
            GenderType senderGender = request.SenderGender;
            ClassType senderClass = request.SenderClass;
            HairColorType senderHairColor = request.SenderHairColor;
            HairStyleType senderHairStyle = request.SenderHairStyle;

            var newSenderMail = new CharacterNoteDto
            {
                Date = DateTime.UtcNow,
                SenderId = senderId,
                ReceiverId = receiverId,
                Title = title,
                Message = message,
                EquipmentPackets = equipmentPackets,
                IsSenderCopy = true,
                IsOpened = false,
                SenderGender = senderGender,
                SenderClass = senderClass,
                SenderHairColor = senderHairColor,
                SenderHairStyle = senderHairStyle,
                SenderName = senderName,
                ReceiverName = receiverName
            };

            CharacterNoteDto senderMail = await _characterNoteDao.SaveAsync(newSenderMail);

            var newRecvMail = new CharacterNoteDto
            {
                Date = DateTime.UtcNow,
                SenderId = senderId,
                ReceiverId = receiverId,
                Title = title,
                Message = message,
                EquipmentPackets = equipmentPackets,
                IsSenderCopy = false,
                IsOpened = false,
                SenderGender = senderGender,
                SenderClass = senderClass,
                SenderHairColor = senderHairColor,
                SenderHairStyle = senderHairStyle,
                SenderName = senderName,
                ReceiverName = receiverName
            };

            CharacterNoteDto recvMail;
            try
            {
                recvMail = await _characterNoteDao.SaveAsync(newRecvMail);
            }
            catch
            {
                return new CreateNoteResponse
                {
                    Status = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            await _messagePublisher.PublishAsync(new NoteReceivedMessage
            {
                SenderNote = senderMail,
                ReceiverNote = recvMail
            });

            return new CreateNoteResponse
            {
                SenderNote = senderMail,
                ReceiverNote = recvMail,
                Status = RpcResponseType.SUCCESS
            };
        }

        public async ValueTask<BasicRpcResponse> OpenNoteAsync(OpenNoteRequest request)
        {
            long noteId = request.NoteId;
            CharacterNoteDto note = await _characterNoteDao.GetByIdAsync(noteId);
            if (note == null)
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            if (note.IsSenderCopy)
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            if (note.IsOpened)
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            note.IsOpened = true;
            try
            {
                await _characterNoteDao.SaveAsync(note);
            }
            catch
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            return new BasicRpcResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };
            ;
        }

        public async ValueTask<BasicRpcResponse> RemoveNoteAsync(RemoveNoteRequest request)
        {
            long noteId = request.NoteId;
            CharacterNoteDto note = await _characterNoteDao.GetByIdAsync(noteId);
            if (note == null)
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            try
            {
                await _characterNoteDao.DeleteByIdAsync(note.Id);
            }
            catch
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            return new BasicRpcResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };
        }
    }
}