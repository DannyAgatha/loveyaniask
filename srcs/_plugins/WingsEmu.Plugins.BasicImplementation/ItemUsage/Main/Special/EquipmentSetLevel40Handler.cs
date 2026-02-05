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

public class EquipmentSetLevel40Handler : IItemHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    
    public EquipmentSetLevel40Handler(IGameItemInstanceFactory gameItemInstanceFactory) => _gameItemInstanceFactory = gameItemInstanceFactory;
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 30012 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e == null)
        {
            throw new ArgumentNullException(nameof(e));
        }
        
        // Swordsman
        const int Broad_Sword_of_Spirit = 24;
        const int Masterpiece_Crossbow = 74;
        const int Scale_Armour = 101;
        
        // Archer
        const int Wind_Bow = 38;
        const int Masterpiece_Dirk = 82;
        const int Cow_Leather_Tunic = 114;
        
        // Mage
        const int Magic_Wand_of_Light = 52;
        const int Masterpiece_Spell_Gun = 90;
        const int Muslin_Robe = 127;

        var classItemToAdd = new Dictionary<ClassType, (int, int, int)>
        {
            { ClassType.Swordman, (Broad_Sword_of_Spirit, Masterpiece_Crossbow, Scale_Armour)},
            { ClassType.Archer, (Wind_Bow, Masterpiece_Dirk, Cow_Leather_Tunic)},
            { ClassType.Magician, (Magic_Wand_of_Light, Masterpiece_Spell_Gun, Muslin_Robe)}
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