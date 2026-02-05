using System.Collections.Generic;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Configurations;

public class ChangeClassConfiguration
{
    private readonly IReadOnlyDictionary<ClassType, ChangeClassByTypeConfig> _configByClass;

    public ChangeClassConfiguration(IEnumerable<ChangeClassByTypeConfig> config)
    {
        Dictionary<ClassType, ChangeClassByTypeConfig> configByClass = new();
        
        foreach (ChangeClassByTypeConfig changeClassByTypeConfig in config)
        {
            configByClass.TryAdd(changeClassByTypeConfig.ClassType, changeClassByTypeConfig);
        }

        _configByClass = configByClass;
    }

    public ChangeClassByTypeConfig GetConfigByClassType(ClassType classType) => _configByClass.GetValueOrDefault(classType);
}

public class ChangeClassByTypeConfig
{
    public ClassType ClassType { get; init; }
    public List<ChangeClassConfigItem> MainWeapons { get; init; }
    public List<ChangeClassConfigItem> SecondWeapons { get; init; }
    public List<ChangeClassConfigItem> Armors { get; init; }
    public List<ChangeClassConfigItem> Specialists { get; init; }
    public List<ChangeClassConfigItem> WeaponSkins { get; init; }
}

public class ChangeClassConfigItem
{
    public byte? Level { get; init; }
    public byte? HeroLevel { get; init; }
    public short ItemVnum { get; init; }
    public byte? Type { get; init; }
    public string? Category { get; init; } 
}