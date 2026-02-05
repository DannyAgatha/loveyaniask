using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class AddStaticBonusEvent : PlayerEvent
{
    public AddStaticBonusEvent(CharacterStaticBonusDto staticBonusDto) => StaticBonusDto = staticBonusDto;

    public CharacterStaticBonusDto StaticBonusDto { get; }
}