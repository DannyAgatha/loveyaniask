using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;

namespace WingsAPI.Game.Extensions.CharacterExtensions
{
    public static class ManagerExtensions
    {
        public static void RemoveMeditation(this IPlayerEntity character, IMeditationManager meditationManager)
        {
            if (!meditationManager.HasMeditation(character))
            {
                return;
            }

            meditationManager.RemoveAllMeditation(character);
        }
    }
}