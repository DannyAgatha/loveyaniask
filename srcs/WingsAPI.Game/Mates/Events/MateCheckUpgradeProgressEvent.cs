using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Mates.Events
{
    public class MateCheckUpgradeProgressEvent : PlayerEvent
    {
        public MateCheckUpgradeProgressEvent(IMateEntity mateEntity, IMonsterEntity mateDoll, bool isAttackProgress)
        {
            MateEntity = mateEntity;
            IsAttackProgress = isAttackProgress;
            MateDoll = mateDoll;
        }

        public IMateEntity MateEntity { get; }
        public IMonsterEntity MateDoll { get; }
        public bool IsAttackProgress { get; }
    }
}