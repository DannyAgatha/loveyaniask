using System.Collections.Generic;
using WingsEmu.Core.Extensions;

namespace WingsEmu.Game.Configurations;

public interface IChestDropItemConfig
{
    ChestDropItemConfiguration GetChestByDataId(int id);
}

public class ChestDropItemConfig : IChestDropItemConfig
{
    private readonly Dictionary<int, ChestDropItemConfiguration> _configs = new();

    public ChestDropItemConfig(IEnumerable<ChestDropItemConfiguration> configurations)
    {
        foreach (ChestDropItemConfiguration config in configurations)
        {
            _configs[config.Id] = config;
        }
    }

    public ChestDropItemConfiguration GetChestByDataId(int id) => _configs.GetOrDefault(id);
}

public class ChestDropItemConfiguration
{
    public short Id { get; set; }
    public int ItemChance { get; set; }
    public List<ChestDropItemDrop> PossibleItems { get; set; }
}

public class ChestDropItemDrop
{
    public int ItemVnum { get; set; }
    public int Amount { get; set; }
}