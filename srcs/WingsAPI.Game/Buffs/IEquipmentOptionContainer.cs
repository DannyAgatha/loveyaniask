// NosEmu
// 


using System.Collections.Generic;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Buffs;

public interface IEquipmentOptionContainer
{
    public void AddShells(EquipmentOptionType equipmentOptionType, List<EquipmentOptionDTO> optionDto, bool isMainWeapon);
    public void ClearShells(EquipmentOptionType equipmentOptionType, bool isMainWeapon);
    public Dictionary<ShellEffectType, int> GetShellsValues(EquipmentOptionType equipmentOptionType, bool isMainWeapon);
    public int GetMaxWeaponShellValue(ShellEffectType shellEffectType, bool isMainWeapon);
    public int GetMaxArmorShellValue(ShellEffectType shellEffectType);

    public void AddCellon(EquipmentType equipmentType, List<EquipmentOptionDTO> optionDto);
    public void ClearCellon(EquipmentType equipmentType);
    public Dictionary<CellonType, int> GetCellonValues(EquipmentType equipmentType);
    public int GetCellonValue(EquipmentType equipmentType, CellonType type);
}