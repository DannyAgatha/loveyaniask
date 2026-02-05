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

public class EquipmentSetLevel50Handler : IItemHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    
    public EquipmentSetLevel50Handler(IGameItemInstanceFactory gameItemInstanceFactory) => _gameItemInstanceFactory = gameItemInstanceFactory;
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 30013 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e == null)
        {
            throw new ArgumentNullException(nameof(e));
        }
        
        // Swordsman
        const int Longsword_of_Power = 28;
        const int Beechwood_Crossbow = 76;
        const int Plate_Armour = 103;
        
        // Archer
        const int Bow_of_Courage = 42;
        const int Ruby_Dagger = 84;
        const int Peccary_Tunic = 116;
        
        // Mage
        const int Magic_Wand_of_Honour = 56;
        const int Dignified_Spell_Gun = 92;
        const int Silk_Robe = 129;

        var classItemToAdd = new Dictionary<ClassType, (int, int, int)>
        {
            { ClassType.Swordman, (Longsword_of_Power, Beechwood_Crossbow, Plate_Armour)},
            { ClassType.Archer, (Bow_of_Courage, Ruby_Dagger, Peccary_Tunic)},
            { ClassType.Magician, (Magic_Wand_of_Honour, Dignified_Spell_Gun, Silk_Robe)}
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