using System;

namespace WingsEmu.Game.Entities
{
    public interface IEntityInteraction
    {
        public bool IsJumping { get; set; }
        public bool HasSpawnRedCircle { get;set; }
        public DateTime LastJump { get; set; }
        short? SkillToUse { get; set; }
        bool ForceUseSkill { get; set; }
    }
}