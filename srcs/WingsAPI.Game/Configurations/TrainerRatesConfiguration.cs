using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class TrainerRatesConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int ExtractEssenceRate { get; set; } = 1;
}