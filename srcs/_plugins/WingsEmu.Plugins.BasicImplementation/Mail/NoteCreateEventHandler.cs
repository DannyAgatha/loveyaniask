using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Mail;
using WingsAPI.Data.Character;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Mail;

public class NoteCreateEventHandler : IAsyncEventProcessor<NoteCreateEvent>
{
    private readonly ICharacterService _characterService;
    private readonly IGameLanguageService _gameLanguage;
    private readonly INoteService _noteService;

    public NoteCreateEventHandler(INoteService noteService, IGameLanguageService gameLanguage, ICharacterService characterService)
    {
        _noteService = noteService;
        _gameLanguage = gameLanguage;
        _characterService = characterService;
    }

    public async Task HandleAsync(NoteCreateEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        string receiverName = e.ReceiverName;
        string title = e.Title;
        string message = e.Message;

        if (session.PlayerEntity.Name == receiverName)
        {
            return;
        }

        if (session.PlayerEntity.LastSentNote.AddSeconds(5) > DateTime.UtcNow)
        {
            return;
        }

        DbServerGetCharacterResponse characterResponse = null;
        try
        {
            characterResponse = await _characterService.GetCharacterByName(new DbServerGetCharacterRequestByName
            {
                CharacterName = receiverName
            });
        }
        catch (Exception ex)
        {
            Log.Error("[NOTE_CREATE] Unexpected error: ", ex);
        }

        if (characterResponse?.RpcResponseType != RpcResponseType.SUCCESS)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_USER_NOT_FOUND, session.UserLanguage));
            return;
        }

        CharacterDTO receiver = characterResponse.CharacterDto;

        if (title.Length > 30)
        {
            title = title.Substring(0, 30);
        }

        if (message.Length > 200)
        {
            message = message.Substring(0, 200);
        }

        session.PlayerEntity.LastSentNote = DateTime.UtcNow;
        CreateNoteResponse response = await _noteService.CreateNoteAsync(new CreateNoteRequest
        {
            ReceiverId = receiver.Id,
            ReceiverName = receiverName,
            SenderId = session.PlayerEntity.Id,
            SenderName = session.PlayerEntity.Name,
            EquipmentPackets = session.GenerateEqListForPacket(),
            Title = title,
            Message = message,
            SenderClass = session.PlayerEntity.Class,
            SenderGender = session.PlayerEntity.Gender,
            SenderHairColor = session.PlayerEntity.HairColor,
            SenderHairStyle = session.PlayerEntity.HairStyle
        });

        if (response.Status != RpcResponseType.SUCCESS)
        {
            return;
        }
        
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.NoteSent);
        
        await session.EmitEventAsync(new NoteSentEvent
        {
            NoteId = response.SenderNote.Id,
            ReceiverName = receiverName,
            Message = message,
            Title = title
        });
    }
}