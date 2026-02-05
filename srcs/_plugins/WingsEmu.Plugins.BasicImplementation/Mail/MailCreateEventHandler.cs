using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Mail;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Mails;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails.Events;

namespace NosEmu.Plugins.BasicImplementations.Mail;

public class MailCreateEventHandler : IAsyncEventProcessor<MailCreateEvent>
{
    private readonly IGameItemInstanceFactory _itemInstanceFactory;
    private readonly MailCreationManager _mailCreationManager;

    public MailCreateEventHandler(MailCreationManager mailCreationManager, IGameItemInstanceFactory itemInstanceFactory)
    {
        _mailCreationManager = mailCreationManager;
        _itemInstanceFactory = itemInstanceFactory;
    }

    public async Task HandleAsync(MailCreateEvent e, CancellationToken cancellation)
    {
        GameItemInstance itemInstance = e.ItemInstance;
        string senderName = e.SenderName;
        long receiverId = e.ReceiverId;
        MailGiftType mailGiftType = e.MailGiftType;

        if (itemInstance == null)
        {
            return;
        }

        ItemInstanceDTO itemInstanceDto = _itemInstanceFactory.CreateDto(itemInstance);
        _mailCreationManager.AddCreateMailRequest(new CreateMailRequest
        {
            SenderName = senderName,
            ReceiverId = receiverId,
            ItemInstance = itemInstanceDto,
            MailGiftType = mailGiftType
        });
    }
}