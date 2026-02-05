using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateProcessExperienceEvent : PlayerEvent
{
    public MateProcessExperienceEvent(IMateEntity mateEntity, long experience)
    {
        MateEntity = mateEntity;
        Experience = experience;
    }

    public IMateEntity MateEntity { get; }
    public long Experience { get; }
}