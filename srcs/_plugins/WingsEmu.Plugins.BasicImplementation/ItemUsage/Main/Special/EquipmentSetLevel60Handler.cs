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

public class EquipmentSetLevel60Handler : IItemHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    
    public EquipmentSetLevel60Handler(IGameItemInstanceFactory gameItemInstanceFactory) => _gameItemInstanceFactory = gameItemInstanceFactory;
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 30014 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e == null)
        {
            throw new ArgumentNullException(nameof(e));
        }
        
        // Swordsman
        const int Claymore = 31;
        const int Elven_Crossbow = 760;
        const int Armour_of_Valour = 106;
        
        // Archer
        const int Bow_of_Spirit = 45;
        const int Elven_Dagger = 762;
        const int Spirit_Tunic = 119;
        
        // Mage
        const int Magic_Wand_of_Shadows = 59;
        const int Spirit_Spell_Gun = 764;
        const int Robe_of_Darkness = 132;

        var classItemToAdd = new Dictionary<ClassType, (int, int, int)>
        {
            { ClassType.Swordman, (Claymore, Elven_Crossbow, Armour_of_Valour)},
            { ClassType.Archer, (Bow_of_Spirit, Elven_Dagger, Spirit_Tunic)},
            { ClassType.Magician, (Magic_Wand_of_Shadows, Spirit_Spell_Gun, Robe_of_Darkness)}
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