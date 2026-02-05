// NosEmu
// 


using WingsEmu.Game.Helpers;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.CharacterExtensions
{
    public static class PlayerEntityExtensions
    {
        public static Location GetLocation(this IClientSession playerEntity)
        {
            Position position = playerEntity.PlayerEntity.Position;
            if (playerEntity.CurrentMapInstance is null)
            {
                return new Location(playerEntity.PlayerEntity.MapId, position.X, position.Y);
            }

            return new Location(playerEntity.CurrentMapInstance.MapId, position.X, position.Y);
        }
    }
}