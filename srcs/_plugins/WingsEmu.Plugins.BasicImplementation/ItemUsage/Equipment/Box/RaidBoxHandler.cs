using System.Threading.Tasks;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Equipment.Box;

public class RaidBoxHandler : IItemHandler
{
    private readonly IGameLanguageService _languageService;

    public RaidBoxHandler(IGameLanguageService languageService) => _languageService = languageService;

    public ItemType ItemType => ItemType.Box;
    public long[] Effects => [303];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        InventoryItem box = session.PlayerEntity.GetItemBySlotAndType(e.Item.Slot, InventoryType.Equipment);
        if (box == null)
        {
            return;
        }

        if (box.ItemInstance.Type != ItemInstanceType.BoxInstance)
        {
            return;
        }

        GameItemInstance boxItem = box.ItemInstance;

        if (boxItem.GameItem.ItemSubType != 3)
        {
            return;
        }

        await session.EmitEventAsync(new RollItemBoxEvent
        {
            Item = box
        });
    }
}