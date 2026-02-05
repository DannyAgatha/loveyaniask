using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class PlayerChangeChannelEvent : PlayerEvent
{
    public PlayerChangeChannelEvent(SerializableGameServer gameServer, ItModeType modeType, int mapId, short mapX, short mapY)
    {
        GameServer = gameServer;
        ModeType = modeType;
        MapId = mapId;
        MapX = mapX;
        MapY = mapY;
    }

    public SerializableGameServer GameServer { get; }
    public ItModeType ModeType { get; }

    public int MapId { get; }
    public short MapX { get; }
    public short MapY { get; }
}