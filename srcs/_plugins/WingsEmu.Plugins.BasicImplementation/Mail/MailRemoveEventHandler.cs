using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication;
using WingsAPI.Communication.Mail;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game.Mails;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Mail;

public class MailRemoveEventHandler : IAsyncEventProcessor<MailRemoveEvent>
{
    private readonly IMailService _mailService;

    public MailRemoveEventHandler(IMailService mailService) => _mailService = mailService;

    public async Task HandleAsync(MailRemoveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        long mailId = e.MailId;

        CharacterMail mail = session.PlayerEntity.MailNoteComponent.GetMail(mailId);
        if (mail == null)
        {
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

        session.SendParcelDelete(7, mailId);
        session.PlayerEntity.MailNoteComponent.RemoveMail(mail);

        await session.EmitEventAsync(new MailRemovedEvent
        {
            MailId = mail.Id,
            ItemInstance = mail.ItemInstance,
            SenderName = mail.SenderName
        });
    }
}