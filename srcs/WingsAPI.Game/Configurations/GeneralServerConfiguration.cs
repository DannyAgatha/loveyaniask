using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations;

public class StaticGeneralServerConfiguration
{
    public static GeneralServerConfiguration Instance { get; private set; }

    public static void Initialize(GeneralServerConfiguration instance)
    {
        Instance = instance;
    }
}

[DataContract]
public class GeneralServerConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int MaxItemAmount { get; set; } = 9999;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public bool CanDeleteDailyQuest { get; set; } = true;
}