using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Teacher;

public class PartnerFoodHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemUsageManager _itemUsageManager;

    public PartnerFoodHandler(IItemUsageManager itemUsageManager, IGameLanguageService gameLanguage)
    {
        _itemUsageManager = itemUsageManager;
        _gameLanguage = gameLanguage;
    }

    public ItemType ItemType => ItemType.PetPartnerItem;
    public long[] Effects => new long[] { 10002 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!long.TryParse(e.Packet[3], out long mateId))
        {
            return;
        }

        IMateEntity mate = session.PlayerEntity.MateComponent.GetMate(m => m.Id == mateId && m.MateType == MateType.Partner);

        if (mate == null)
        {
            return;
        }

        if (!mate.IsAlive())
        {
            return;
        }

        if (mate.Loyalty == 1000)
        {
            return;
        }

        int loyalty = mate.Loyalty + 100 > 1000 ? 1000 - mate.Loyalty : 100;
        mate.Loyalty += (short)loyalty;
        session.SendCondMate(mate);
        session.SendPetInfo(mate, _gameLanguage);
        session.SendMateEffect(mate, EffectType.PetLove);
        session.SendMateEffect(mate, EffectType.ShinyStars);
        session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_CHATMESSAGE_EAT_EVERYTHING, session.UserLanguage), ChatMessageColorType.Yellow);
        await session.RemoveItemFromInventory(item: e.Item);
    }
}