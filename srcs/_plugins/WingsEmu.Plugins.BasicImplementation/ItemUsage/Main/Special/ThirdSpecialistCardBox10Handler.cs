using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class ThirdSpecialistCardBox10Handler : IItemHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    
    public ThirdSpecialistCardBox10Handler(IGameItemInstanceFactory gameItemInstanceFactory) => _gameItemInstanceFactory = gameItemInstanceFactory;
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 30006 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity == null)
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.CANNOT_BE_USED));
            return;
        }
        
        var classItemToAdd = new Dictionary<ClassType, int>
        {
            { ClassType.Swordman, 909 },
            { ClassType.Archer, 911},
            { ClassType.Magician, 913 }
        };

        if (!classItemToAdd.TryGetValue(session.PlayerEntity.Class, out int specialistItemVNum))
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.CANNOT_BE_USED));
            return;
        }
        
        GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(specialistItemVNum, 1, 7);

        await session.RemoveItemFromInventory(item: e.Item);
        await session.AddNewItemToInventory(newItem, true, sendGiftIsFull: true);
        session.SendRdiPacket(newItem.ItemVNum, (short)newItem.Amount);
    }
}