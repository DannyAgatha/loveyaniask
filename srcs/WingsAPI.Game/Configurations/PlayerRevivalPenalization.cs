using System.Runtime.Serialization;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class PlayerRevivalPenalization
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public byte MaxLevelWithoutRevivalPenalization { get; set; } = 20;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int BaseMapRevivalPenalizationSaver { get; set; } = (int)ItemVnums.SEED_OF_POWER;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int BaseMapRevivalPenalizationSaverAmount { get; set; } = 10;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int BaseMapRevivalPenalizationDebuff { get; set; } = (int)BuffVnums.RESURRECTION_SIDE_EFFECTS;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public byte MaxLevelWithDignityPenalizationIncrement { get; set; } = 50;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public byte DignityPenalizationIncrementMultiplier { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public long ArenaGoldPenalization { get; set; } = 100;
}