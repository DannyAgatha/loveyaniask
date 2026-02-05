using System.Collections.Generic;
using System.Linq;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Buffs;

public class EquipmentOptionContainer : IEquipmentOptionContainer
{
    private readonly Dictionary<EquipmentType, Dictionary<CellonType, int>> _cellons;
    private readonly Dictionary<(bool isMainWeapon, EquipmentOptionType equipmentOptionType), Dictionary<ShellEffectType, int>> _shells;

    public EquipmentOptionContainer()
    {
        _cellons = new Dictionary<EquipmentType, Dictionary<CellonType, int>>();
        _shells = new Dictionary<(bool isMainWeapon, EquipmentOptionType equipmentOptionType), Dictionary<ShellEffectType, int>>();
    }

    public void AddShells(EquipmentOptionType equipmentOptionType, List<EquipmentOptionDTO> optionDto, bool isMainWeapon)
    {
        if (equipmentOptionType != EquipmentOptionType.ARMOR_SHELL && equipmentOptionType != EquipmentOptionType.WEAPON_SHELL)
        {
            return;
        }

        if (optionDto == null)
        {
            return;
        }

        if (!optionDto.Any())
        {
            return;
        }

        if (!_shells.TryGetValue((isMainWeapon, equipmentOptionType), out Dictionary<ShellEffectType, int> dictionary))
        {
            dictionary = new Dictionary<ShellEffectType, int>();
            _shells[(isMainWeapon, equipmentOptionType)] = dictionary;
        }

        foreach (EquipmentOptionDTO option in optionDto)
        {
            var shellOptionType = (ShellEffectType)option.Type;
            dictionary[shellOptionType] = option.Value;
        }
    }

    public void ClearShells(EquipmentOptionType equipmentOptionType, bool isMainWeapon)
    {
        if (!_shells.TryGetValue((isMainWeapon, equipmentOptionType), out Dictionary<ShellEffectType, int> dictionary))
        {
            return;
        }

        dictionary.Clear();
    }

    public Dictionary<ShellEffectType, int> GetShellsValues(EquipmentOptionType equipmentOptionType, bool isMainWeapon) =>
        !_shells.TryGetValue((isMainWeapon, equipmentOptionType), out Dictionary<ShellEffectType, int> dictionary) ? new Dictionary<ShellEffectType, int>() : dictionary;

    public int GetMaxWeaponShellValue(ShellEffectType shellEffectType, bool isMainWeapon)
    {
        if (!_shells.TryGetValue((isMainWeapon, EquipmentOptionType.WEAPON_SHELL), out Dictionary<ShellEffectType, int> dictionary))
        {
            return default;
        }

        return dictionary.GetOrDefault(shellEffectType);
    }

    public int GetMaxArmorShellValue(ShellEffectType shellEffectType)
    {
        if (!_shells.TryGetValue((false, EquipmentOptionType.ARMOR_SHELL), out Dictionary<ShellEffectType, int> dictionary))
        {
            return default;
        }

        return dictionary.GetOrDefault(shellEffectType);
    }

    public void AddCellon(EquipmentType equipmentType, List<EquipmentOptionDTO> optionDto)
    {
        if (equipmentType != EquipmentType.Necklace && equipmentType != EquipmentType.Bracelet && equipmentType != EquipmentType.Ring)
        {
            return;
        }

        if (optionDto == null)
        {
            return;
        }

        if (!optionDto.Any())
        {
            return;
        }

        if (!_cellons.TryGetValue(equipmentType, out Dictionary<CellonType, int> dictionary))
        {
            dictionary = new Dictionary<CellonType, int>();
            _cellons[equipmentType] = dictionary;
        }

        foreach (EquipmentOptionDTO option in optionDto)
        {
            dictionary[(CellonType)option.Type] = option.Value;
        }
    }

    public void ClearCellon(EquipmentType equipmentType)
    {
        if (!_cellons.TryGetValue(equipmentType, out Dictionary<CellonType, int> dictionary))
        {
            return;
        }

        dictionary.Clear();
    }

    public Dictionary<CellonType, int> GetCellonValues(EquipmentType equipmentType)
    {
        if (!_cellons.TryGetValue(equipmentType, out Dictionary<CellonType, int> dictionary))
        {
            return new Dictionary<CellonType, int>();
        }

        return dictionary;
    }

    public int GetCellonValue(EquipmentType equipmentType, CellonType type)
    {
        if (!_cellons.TryGetValue(equipmentType, out Dictionary<CellonType, int> dictionary))
        {
            return default;
        }

        return dictionary.GetOrDefault(type);
    }
}