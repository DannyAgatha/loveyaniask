using System.Collections.Generic;
using WingsEmu.DTOs.Items;

namespace WingsEmu.Game.Algorithm;

public interface IShellGenerationAlgorithm
{
    /// <summary>
    /// </summary>
    /// <param name="shellType"></param>
    /// <param name="shellRarity"></param>
    /// <param name="shellLevel"></param>
    /// <returns></returns>
    IEnumerable<EquipmentOptionDTO> GenerateShell(byte shellType, int shellRarity, int shellLevel);
}