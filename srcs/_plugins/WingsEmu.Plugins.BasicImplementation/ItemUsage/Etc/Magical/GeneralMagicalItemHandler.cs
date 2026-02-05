// NosEmu
// 


using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

public class GeneralMagicalItemHandler : IItemHandler
{
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;

    public GeneralMagicalItemHandler(IBCardEffectHandlerContainer bCardEffectHandlerContainer) => _bCardEffectHandlerContainer = bCardEffectHandlerContainer;

    public ItemType ItemType => ItemType.Magical;
    public long[] Effects => new long[] { 0 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        InventoryItem item = e.Item;
        switch ((ItemVnums)item.ItemInstance.ItemVNum)
        {
            case ItemVnums.LIMITED_BANK_CARD:
            case ItemVnums.BANK_CARD:
                await session.EmitEventAsync(new BankOpenEvent
                {
                    BankCard = e.Item
                });
                return;
            case ItemVnums.BANDAGE:

                if (!session.PlayerEntity.BuffComponent.HasAnyBuff())
                {
                    return;
                }

                foreach (BCardDTO bCard in item.ItemInstance.GameItem.BCards)
                {
                    _bCardEffectHandlerContainer.Execute(session.PlayerEntity, session.PlayerEntity, bCard);
                }

                await session.RemoveItemFromInventory(item: item);

                break;
        }
    }
}