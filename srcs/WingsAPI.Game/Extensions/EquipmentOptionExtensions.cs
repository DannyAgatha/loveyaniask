// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Items;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Extensions;

public static class EquipmentOptionExtensions
{
    public static int GetJewelsCellonsValue(this IPlayerEntity character, CellonType cellonType)
    {
        int ring = character.GetCellonValue(EquipmentType.Ring, cellonType);
        int necklace = character.GetCellonValue(EquipmentType.Necklace, cellonType);
        int bracelet = character.GetCellonValue(EquipmentType.Bracelet, cellonType);

        return ring + necklace + bracelet;
    }

    /// <summary>
    ///     Picks the maximum values between primary weapon and secondary weapon for the given shell optionType
    /// </summary>
    /// <param name="character"></param>
    /// <param name="shellEffectType"></param>
    /// <returns></returns>
    public static int GetMaxWeaponShellValue(this IPlayerEntity character, ShellEffectType shellEffectType)
    {
        int mainValue = character.GetMaxWeaponShellValue(shellEffectType, true);
        int secondValue = character.GetMaxWeaponShellValue(shellEffectType, false);
        if (mainValue == 0 && secondValue == 0)
        {
            return 0;
        }

        return mainValue >= secondValue ? mainValue : secondValue;
    }

    public static List<BCardDTO> ShellBuffs(this IPlayerEntity playerEntity, GameItemInstance gameItemInstance)
    {
        List<EquipmentOptionDTO> options = gameItemInstance.EquipmentOptions;
        if (options == null)
        {
            return null;
        }

        if (!options.Any())
        {
            return null;
        }

        List<BCardDTO> buffBCards = new();
        foreach (EquipmentOptionDTO option in options)
        {
            if (option.EquipmentOptionType != EquipmentOptionType.WEAPON_SHELL)
            {
                continue;
            }

            var type = (ShellEffectType)option.Type;

            BCardDTO newBCard = type.TryCreateBuffBCard(option.Value);
            if (newBCard == null)
            {
                continue;
            }

            buffBCards.Add(newBCard);
        }

        return buffBCards;
    }

    public static void TryAddShellBuffs(this IPlayerEntity playerEntity, GameItemInstance gameItemInstance)
    {
        List<BCardDTO> buffBCards = playerEntity.ShellBuffs(gameItemInstance);
        if (buffBCards == null)
        {
            return;
        }

        if (!buffBCards.Any())
        {
            return;
        }

        playerEntity.BCardComponent.AddShellTrigger(gameItemInstance.GameItem.EquipmentSlot == EquipmentType.MainWeapon, buffBCards);
    }
}