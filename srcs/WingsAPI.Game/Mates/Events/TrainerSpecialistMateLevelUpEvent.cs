using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Mates.Events
{
    public class TrainerSpecialistMateLevelUpEvent : PlayerEvent
    {
        public TrainerSpecialistMateLevelUpEvent(IMateEntity mateEntity, IMonsterEntity sparringMonster)
        {
            MateEntity = mateEntity;
            SparringMonster = sparringMonster;
        }

        public IMateEntity MateEntity { get; }
        public IMonsterEntity SparringMonster { get; }
    }
}