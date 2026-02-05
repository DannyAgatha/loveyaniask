using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class SteampunkBoxV1Handler: IItemHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    public SteampunkBoxV1Handler(IGameItemInstanceFactory gameItemInstanceFactory) => _gameItemInstanceFactory = gameItemInstanceFactory;
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 30002 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        GameItemInstance newItem = null;

        switch (session.PlayerEntity.Class)
        {
            case ClassType.Swordman:
                newItem = _gameItemInstanceFactory.CreateItem(30021, 1); // Steampunk Chronoblade V1
                break;
            case ClassType.Archer:
                newItem = _gameItemInstanceFactory.CreateItem(30022, 1); // Steampunk Clockwork Bow V1
                break;
            case ClassType.Magician:
                newItem = _gameItemInstanceFactory.CreateItem(30023, 1); // Steampunk Geared Wand V1
                break;
            default:
                session.SendInfo(session.GetLanguage(GameDialogKey.CANNOT_BE_USED));
                break;
        }

        if (newItem != null)
        {
            await session.RemoveItemFromInventory(item: e.Item);
            await session.AddNewItemToInventory(newItem, true, sendGiftIsFull: true);
            session.SendPacket($"rdi {newItem.ItemVNum} {newItem.Amount}");
        }
    }
}