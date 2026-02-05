namespace WingsEmu.Game.Helpers.Damages
{
    public class DamageAlgorithmResult
    {
        public DamageAlgorithmResult(int damages, HitType hitMode, bool isSoftDamageEffect,  bool specialistDragonEffect)
        {
            Damages = damages;
            HitType = hitMode;
            SoftDamageEffect = isSoftDamageEffect;
            SpecialistDragonEffect = specialistDragonEffect;
        }

        public bool SoftDamageEffect { get; }
        public int Damages { get; set; }
        public HitType HitType { get; }
        public bool SpecialistDragonEffect { get; set; }
    }

    public enum HitType
    {
        Normal,
        Critical,
        Miss
    }
}
