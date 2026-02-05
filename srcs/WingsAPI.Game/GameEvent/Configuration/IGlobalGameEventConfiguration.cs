using WingsEmu.Game.Characters;

namespace WingsEmu.Game.GameEvent.Configuration;

public interface IGlobalGameEventConfiguration
{
    IGameEventConfiguration GetConfiguration(IPlayerEntity character);

    public uint GetRegistrationCost(IPlayerEntity character);
}