using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
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

public class EquipmentSetLevel20Handler : IItemHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    
    public EquipmentSetLevel20Handler(IGameItemInstanceFactory gameItemInstanceFactory) => _gameItemInstanceFactory = gameItemInstanceFactory;
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 30010 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e == null)
        {
            throw new ArgumentNullException(nameof(e));
        }
        
        // Swordsman
        const int Gladius = 135;
        const int Combat_Crossbow = 156;
        const int Durable_Armour = 162;
        
        // Archer
        const int Bow_of_Life = 142;
        const int Highlander_Dirk = 158;
        const int Flash_Tunic = 168;
        
        // Mage
        const int Magic_Wand_of_Life = 149;
        const int Normal_Spell_Gun = 160;
        const int Beginners_Robe = 174;

        var classItemToAdd = new Dictionary<ClassType, (int, int, int)>
        {
            { ClassType.Swordman, (Gladius, Combat_Crossbow, Durable_Armour)},
            { ClassType.Archer, (Bow_of_Life, Highlander_Dirk, Flash_Tunic)},
            { ClassType.Magician, (Magic_Wand_of_Life, Normal_Spell_Gun, Beginners_Robe)}
        };

        if (classItemToAdd.TryGetValue(session.PlayerEntity.Class, out (int, int, int) itemValues))
        {
            int mainWeaponVNum = itemValues.Item1;
            int secondaryWeaponVNum = itemValues.Item2;
            int armourWeaponVNum = itemValues.Item3;

            GameItemInstance mainWeapon = _gameItemInstanceFactory.CreateItem(mainWeaponVNum, 1, 5, 5);
            GameItemInstance secondaryWeapon = _gameItemInstanceFactory.CreateItem(secondaryWeaponVNum, 1, 5, 5);
            GameItemInstance armour = _gameItemInstanceFactory.CreateItem(armourWeaponVNum, 1, 5, 5);
            
            await session.RemoveItemFromInventory(item: e.Item);
            await session.AddNewItemToInventory(mainWeapon, true, sendGiftIsFull: true);
            await session.AddNewItemToInventory(secondaryWeapon, true, sendGiftIsFull: true);
            await session.AddNewItemToInventory(armour, true, sendGiftIsFull: true);
        }
        else
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.CANNOT_BE_USED));
        }
    }
}