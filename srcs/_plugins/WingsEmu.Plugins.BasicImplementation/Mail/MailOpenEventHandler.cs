using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication;
using WingsAPI.Communication.Mail;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Mail;

public class MailOpenEventHandler : IAsyncEventProcessor<MailOpenEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMailService _mailService;

    public MailOpenEventHandler(IMailService mailService, IGameLanguageService gameLanguage)
    {
        _mailService = mailService;
        _gameLanguage = gameLanguage;
    }

    public async Task HandleAsync(MailOpenEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        long mailId = e.MailId;
        CharacterMail mail = session.PlayerEntity.MailNoteComponent.GetMail(mailId);
        GameItemInstance itemInstance = mail?.ItemInstance;
        if (itemInstance == null)
        {
            return;
        }

        if (!session.PlayerEntity.HasSpaceFor(itemInstance.ItemVNum, (short)itemInstance.Amount))
        {
            session.SendParcelDelete(5, mailId);
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage));
            return;
        }

        BasicRpcResponse response = await _mailService.RemoveMailAsync(new RemoveMailRequest
        {
            CharacterId = session.PlayerEntity.Id,
            MailId = mail.Id
        });

        if (response.ResponseType != RpcResponseType.SUCCESS)
        {
            return;
        }

        await session.AddNewItemToInventory(itemInstance);
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.ParcelReceived, 2, itemInstance.ItemVNum.ToString(), itemInstance.Amount);
        session.SendParcelDelete(2, mailId);
        session.PlayerEntity.MailNoteComponent.RemoveMail(mail);

        await session.EmitEventAsync(new MailClaimedEvent
        {
            MailId = mail.Id,
            ItemInstance = itemInstance,
            SenderName = mail.SenderName
        });
    }
}