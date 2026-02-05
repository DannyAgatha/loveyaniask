using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.MultiLanguage;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.InterChannel;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Event;

public class InterChannelReceiveWhisperEventHandler : IAsyncEventProcessor<InterChannelReceiveWhisperEvent>
{
    private readonly IGameLanguageService _languageService;

    public InterChannelReceiveWhisperEventHandler(IGameLanguageService languageService) => _languageService = languageService;

    public async Task HandleAsync(InterChannelReceiveWhisperEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.WhisperBlocked && e.AuthorityType < AuthorityType.GM)
        {
            await e.Sender.EmitEventAsync(new InterChannelSendChatMsgByCharIdEvent(e.SenderCharacterId, GameDialogKey.INFORMATION_CHATMESSAGE_USER_WHISPER_BLOCKED, ChatMessageColorType.Red));
            return;
        }

        if (e.Sender.PlayerEntity.IsBlocking(e.SenderCharacterId) && e.AuthorityType < AuthorityType.GM)
        {
            await e.Sender.EmitEventAsync(new InterChannelSendChatMsgByCharIdEvent(e.SenderCharacterId, GameDialogKey.BLACKLIST_INFO_BLOCKING, ChatMessageColorType.Red));
            return;
        }

        e.Sender.ReceiveSpeakWhisper(e.SenderCharacterId, e.SenderNickname, AddChannelToMessage(e.Message, e.SenderChannelId, e.Sender.UserLanguage),
            e.AuthorityType >= AuthorityType.GM ? SpeakType.GameMaster : SpeakType.Player);
    }

    private string AddChannelToMessage(string message, int senderChannelId, RegionLanguageType userLanguage)
    {
        if (senderChannelId == -1)
        {
            return message;
        }

        return message + $" <{_languageService.GetLanguage(GameDialogKey.INFORMATION_CHANNEL, userLanguage)}: {senderChannelId.ToString()}>";
    }
}