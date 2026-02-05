using System;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.MinilandExtensions
{
    public static class MinigameExtensions
    {
        /// <summary>
        ///     Returns True if the MinigamePoints was removed or False if it wasn't.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="minigamePoints"></param>
        /// <param name="minigameConfiguration"></param>
        /// <returns></returns>
        public static bool RemoveMinigamePoints(this IClientSession session, short minigamePoints, MinigameConfiguration minigameConfiguration)
        {
            short minigamePointsToRemove = Math.Abs(minigamePoints);
            if (minigamePointsToRemove > session.PlayerEntity.MinilandPoint)
            {
                return false;
            }

            session.PlayerEntity.MinilandPoint -= minigamePointsToRemove;
            session.SendMinigamePoints(minigameConfiguration);
            return true;
        }

        public static void AddMinigamePoints(this IClientSession session, short minigamePoints, MinigameConfiguration minigameConfiguration)
        {
            short minigamePointsToRemove = Math.Abs(minigamePoints);

            if (minigameConfiguration.Configuration.MaxmimumMinigamePoints < minigamePointsToRemove + session.PlayerEntity.MinilandPoint)
            {
                session.PlayerEntity.MinilandPoint = (short)minigameConfiguration.Configuration.MaxmimumMinigamePoints;
            }
            else
            {
                session.PlayerEntity.MinilandPoint += minigamePointsToRemove;
            }

            session.SendMinigamePoints(minigameConfiguration);
        }
    }
}