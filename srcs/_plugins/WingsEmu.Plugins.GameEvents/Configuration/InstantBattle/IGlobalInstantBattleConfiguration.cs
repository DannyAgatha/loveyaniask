using WingsEmu.Game.Characters;
using WingsEmu.Game.GameEvent.Configuration;

namespace WingsEmu.Plugins.GameEvents.Configuration.InstantBattle
{
    public interface IGlobalInstantBattleConfiguration : IGlobalGameEventConfiguration
    {
        InstantBattleConfiguration GetInternalConfiguration(IPlayerEntity sessionCharacter);
    }
}