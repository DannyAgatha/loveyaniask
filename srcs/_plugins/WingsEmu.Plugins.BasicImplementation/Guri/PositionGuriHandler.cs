using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class PositionGuriHandler : IGuriHandler
{
    public long GuriEffectId => 1;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        session.SendDebugMessage($"[GURI] Old position: {session.PlayerEntity.PositionX}/{session.PlayerEntity.PositionY}", ChatMessageColorType.Red);

        if (e.Packet.Length <= 4)
        {
            return;
        }

        if (!short.TryParse(e.Packet[5], out short newX))
        {
            return;
        }

        if (!short.TryParse(e.Packet[6], out short newY))
        {
            return;
        }

        if (session.CurrentMapInstance.IsBlockedZone(newX, newY))
        {
            return;
        }

        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        int distance = session.PlayerEntity.Position.GetDistance(newX, newY);
        if (distance > 30)
        {
            return;
        }

        session.PlayerEntity.TeleportOnMap(newX, newY);
        session.SendDebugMessage($"[GURI] New position: {session.PlayerEntity.PositionX}/{session.PlayerEntity.PositionY}", ChatMessageColorType.Red);
    }
}