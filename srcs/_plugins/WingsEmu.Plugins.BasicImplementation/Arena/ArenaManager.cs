using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game._enum;
using WingsEmu.Game.Arena;
using WingsEmu.Game.Maps;

namespace NosEmu.Plugins.BasicImplementations.Arena;

public class ArenaManager : IArenaManager
{
    private readonly SerializableGameServer _gameServer;
    private readonly IMapManager _mapManager;

    public ArenaManager(IMapManager mapManager, SerializableGameServer gameServer)
    {
        _mapManager = mapManager;
        _gameServer = gameServer;
    }

    public IMapInstance ArenaInstance { get; private set; }
    public IMapInstance FamilyArenaInstance { get; private set; }

    public async Task Initialize()
    {
        if (_gameServer.ChannelType == GameChannelType.ACT_4)
        {
            Log.Warn("[ARENA_MANAGER] Not loading Arena because the channel is act4");
            return;
        }

        ArenaInstance = _mapManager.GenerateMapInstanceByMapId((int)MapIds.ARENA_INDIVIDUAL, MapInstanceType.ArenaInstance);
        Log.Info(ArenaInstance == null ? "[ARENA_MANAGER] Failed to load Arena Map" : "[ARENA_MANAGER] Arena Map Loaded");

        FamilyArenaInstance = _mapManager.GenerateMapInstanceByMapId((int)MapIds.ARENA_FAMILY, MapInstanceType.ArenaInstance);
        Log.Info(FamilyArenaInstance == null ? "[ARENA_MANAGER] Failed to load Family Arena Map" : "[ARENA_MANAGER] Family Arena Map Loaded");
    }
}