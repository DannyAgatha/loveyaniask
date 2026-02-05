using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.InterChannel;

namespace NosEmu.Plugins.BasicImplementations.InterChannel.Consumer;

public class BazaarNotificationMessageConsumer : IMessageConsumer<BazaarNotificationMessage>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;
    private readonly ISessionManager _sessionManager;

    public BazaarNotificationMessageConsumer(ISessionManager sessionManager, IItemsManager itemsManager, IGameLanguageService gameLanguage)
    {
        _sessionManager = sessionManager;
        _itemsManager = itemsManager;
        _gameLanguage = gameLanguage;
    }

    public async Task HandleAsync(BazaarNotificationMessage notification, CancellationToken token)
    {
        IClientSession owner = _sessionManager.GetSessionByCharacterName(notification.OwnerName);
        if (owner == null)
        {
            return;
        }

        string itemName = _itemsManager.GetItem(notification.ItemVnum).GetItemName(_gameLanguage, owner.UserLanguage);
        int amount = notification.Amount;

        owner.SendChatMessage(owner.GetLanguageFormat(GameDialogKey.BAZAAR_CHATMESSAGE_ITEM_SOLD, amount, itemName), ChatMessageColorType.Green);
    }
}