using System;
using System.Collections.Generic;
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

public class SportsWeaponsChestV2Handler : IItemHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    
    public SportsWeaponsChestV2Handler(IGameItemInstanceFactory gameItemInstanceFactory) => _gameItemInstanceFactory = gameItemInstanceFactory;
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 30009 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        GameItemInstance newItem;
    
        if (e == null)
        {
            throw new ArgumentNullException(nameof(e));
        }

        const int BaseballBatSkinV2 = 30368;
        const int RecurveBowSkinV2 = 30369;
        const int TennisRacquetSkinV2 = 30370;

        var classItemToAdd = new Dictionary<ClassType, int>
        {
            { ClassType.Swordman, BaseballBatSkinV2 },
            { ClassType.Archer, RecurveBowSkinV2 },
            { ClassType.Magician, TennisRacquetSkinV2 }
        };

        if (classItemToAdd.TryGetValue(session.PlayerEntity.Class, out int skinItemVNum))
        {
            newItem = _gameItemInstanceFactory.CreateItem(skinItemVNum, 1);
        }
        else
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.CANNOT_BE_USED));
            return;
        }

        await session.RemoveItemFromInventory(item: e.Item);
        await session.AddNewItemToInventory(newItem, true, sendGiftIsFull: true);
        session.SendPacket($"rdi {newItem.ItemVNum} {newItem.Amount} {newItem.Upgrade}");
    }
}