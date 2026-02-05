namespace WingsEmu.Game.BCard;

public interface IBCardDataComponent
{
    public int? MaxCriticals { get; set; }
    public int? CriticalDamageIncreased { get; set; }
    public int? CriticalDamageDecreased { get; set; }
    public int? IncreaseSpeedTick { get; set; }
    public int? DecreaseSpeedTick { get; set; }
    public int? IncreaseHpPerDebuff { get; set; }
    public int? DecreaseHpPerDebuff { get; set; }
    public int? BlockBadBuff { get; set; }
    public byte? MerlingHit { get; set; }
    public int? VoodooDamageStored { get; set; }
    public int? OldMorph { get; set; }
    public int? OldMorphUpgrade { get; set; }
    public int? OldMorphUpgrade2 { get; set; }
    public int? AbsorptionDamage { get; set; }
    public int? SunWolfChanceIncreaseBuffDuration { get; set; }
}

public class BCardDataComponent : IBCardDataComponent
{
    public int? MaxCriticals { get; set; }
    public int? CriticalDamageIncreased { get; set; }
    public int? CriticalDamageDecreased { get; set; }
    public int? IncreaseSpeedTick { get; set; }
    public int? DecreaseSpeedTick { get; set; }
    public int? IncreaseHpPerDebuff { get; set; }
    public int? DecreaseHpPerDebuff { get; set; }
    public int? BlockBadBuff { get; set; }
    public byte? MerlingHit { get; set; }
    public int? VoodooDamageStored { get; set; }
    public int? OldMorph { get; set; }
    public int? OldMorphUpgrade { get; set; }
    public int? OldMorphUpgrade2 { get; set; }
    public int? AbsorptionDamage { get; set; }
    public int? SunWolfChanceIncreaseBuffDuration { get; set; }
}