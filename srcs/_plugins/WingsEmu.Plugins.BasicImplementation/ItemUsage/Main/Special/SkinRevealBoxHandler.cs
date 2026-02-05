using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Configurations.Skin;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class SkinRevealBoxHandler : IItemUsageByVnumHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly SkinRevealConfiguration _config;

    public SkinRevealBoxHandler(IGameItemInstanceFactory gameItemInstanceFactory, SkinRevealConfiguration config)
    {
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _config = config;
    }

    public long[] Vnums => _config.SkinReveal.Select(x => (long)x.ChestVnum).ToArray();

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity == null)
        {
            return;
        }
        
        int? itemVnum = _config.GetItemVnumForClass(e.Item.ItemInstance.ItemVNum, session.PlayerEntity.Class);

        if (!itemVnum.HasValue)
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.CANNOT_BE_USED));
            return;
        }

        GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(itemVnum.Value, 1);
        
        await session.RemoveItemFromInventory(e.Item.ItemInstance.ItemVNum);
        await session.AddNewItemToInventory(newItem, true, sendGiftIsFull: true);
        session.SendRdiPacket(newItem.ItemVNum, (short)newItem.Amount);
    }
}