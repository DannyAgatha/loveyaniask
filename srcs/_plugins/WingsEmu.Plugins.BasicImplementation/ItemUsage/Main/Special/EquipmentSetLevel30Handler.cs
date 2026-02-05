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

public class EquipmentSetLevel30Handler : IItemHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    
    public EquipmentSetLevel30Handler(IGameItemInstanceFactory gameItemInstanceFactory) => _gameItemInstanceFactory = gameItemInstanceFactory;
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 30011 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e == null)
        {
            throw new ArgumentNullException(nameof(e));
        }
        
        // Swordsman
        const int Walloon_Sword = 136;
        const int Steel_Crossbow = 73;
        const int Armour_of_Stamina = 98;
        
        // Archer
        const int Flame_Bow = 143;
        const int Refined_Dirk = 81;
        const int Tunic_of_Evasion = 111;
        
        // Mage
        const int Magic_Wand_of_Flame = 150;
        const int Gold_Spell_Gun = 89;
        const int Recovery_Robe = 124;

        var classItemToAdd = new Dictionary<ClassType, (int, int, int)>
        {
            { ClassType.Swordman, (Walloon_Sword, Steel_Crossbow, Armour_of_Stamina)},
            { ClassType.Archer, (Flame_Bow, Refined_Dirk, Tunic_of_Evasion)},
            { ClassType.Magician, (Magic_Wand_of_Flame, Gold_Spell_Gun, Recovery_Robe)}
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