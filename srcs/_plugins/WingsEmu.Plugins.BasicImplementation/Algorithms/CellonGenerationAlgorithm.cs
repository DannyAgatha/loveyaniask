// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.Items;
using WingsEmu.Game;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Cellons;

namespace NosEmu.Plugins.BasicImplementations.Algorithms;

public class CellonGenerationAlgorithm : ICellonGenerationAlgorithm
{
    private readonly CellonSystemConfiguration _configuration;
    private readonly IRandomGenerator _randomGenerator;

    public CellonGenerationAlgorithm(IRandomGenerator randomGenerator, CellonSystemConfiguration configuration)
    {
        _randomGenerator = randomGenerator;
        _configuration = configuration;
    }

    public EquipmentOptionDTO GenerateOption(int cellonLevel)
    {
        CellonPossibilities dictionary = _configuration.Options.FirstOrDefault(s => s.CellonLevel == cellonLevel);
        if (dictionary == null)
        {
            return null;
        }

        HashSet<CellonOption> list = dictionary.Options;
        int rand = _randomGenerator.RandomNumber(list.Count);

        CellonOption options = list.ElementAt(rand);
        return new EquipmentOptionDTO
        {
            EquipmentOptionType = EquipmentOptionType.JEWELS,
            Value = _randomGenerator.RandomNumber(options.Range.Minimum, options.Range.Maximum),
            Level = (byte)cellonLevel,
            Type = (byte)options.Type
        };
    }
}