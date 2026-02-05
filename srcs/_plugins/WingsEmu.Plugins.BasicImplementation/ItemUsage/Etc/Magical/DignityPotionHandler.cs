using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

public class DignityPotionHandler : IItemHandler
{
    private readonly IGameLanguageService _languageService;
    private readonly GameMinMaxConfiguration _minMaxConfiguration;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;

    public DignityPotionHandler(IGameLanguageService languageService, GameMinMaxConfiguration minMaxConfiguration, IReputationConfiguration reputationConfiguration, IRankingManager rankingManager)
    {
        _languageService = languageService;
        _minMaxConfiguration = minMaxConfiguration;
        _reputationConfiguration = reputationConfiguration;
        _rankingManager = rankingManager;
    }

    public ItemType ItemType => ItemType.Magical;
    public long[] Effects => new long[] { 14 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IPlayerEntity character = session.PlayerEntity;
        IGameItem gameItem = e.Item.ItemInstance.GameItem;

        if (character.IsOnVehicle)
        {
            string message = _languageService.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage);
            session.SendChatMessage(message, ChatMessageColorType.Yellow);
            return;
        }

        if (!character.AddDignity(gameItem.EffectValue, _minMaxConfiguration, _languageService, _reputationConfiguration, _rankingManager.TopReputation))
        {
            return;
        }

        await session.RemoveItemFromInventory(item: e.Item);
    }
}