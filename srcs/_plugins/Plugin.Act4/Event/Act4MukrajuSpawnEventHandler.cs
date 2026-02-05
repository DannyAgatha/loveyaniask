using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Npcs;
using WingsEmu.Game.Triggers;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act4.Event;

public class Act4MukrajuSpawnEventHandler : IAsyncEventProcessor<Act4MukrajuSpawnEvent>
{
    private readonly Act4Configuration _act4Configuration;
    private readonly IAct4Manager _act4Manager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IGameLanguageService _languageService;
    private readonly IMapManager _mapManager;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly ISessionManager _sessionManager;

    public Act4MukrajuSpawnEventHandler(Act4Configuration act4Configuration, IMapManager mapManager, IAsyncEventPipeline asyncEventPipeline, IAct4Manager act4Manager, ISessionManager sessionManager,
        IGameLanguageService languageService, IMonsterEntityFactory monsterEntityFactory, INpcMonsterManager npcMonsterManager)
    {
        _act4Configuration = act4Configuration;
        _mapManager = mapManager;
        _asyncEventPipeline = asyncEventPipeline;
        _act4Manager = act4Manager;
        _sessionManager = sessionManager;
        _languageService = languageService;
        _monsterEntityFactory = monsterEntityFactory;
        _npcMonsterManager = npcMonsterManager;
    }

    public async Task HandleAsync(Act4MukrajuSpawnEvent e, CancellationToken cancellation)
    {
        bool isAngel = e.FactionType == FactionType.Angel;
        MukrajuSpawn mukrajuSpawn = isAngel ? _act4Configuration.AngelMukrajuSpawn : _act4Configuration.DemonMukrajuSpawn;

        int mapId = mukrajuSpawn.MapId;
        int monsterVNum = mukrajuSpawn.MonsterVnum;
        short mapX = mukrajuSpawn.MapX;
        short mapY = mukrajuSpawn.MapY;

        IMapInstance mapToSpawn = _mapManager.GetBaseMapInstanceByMapId(mapId);
        if (mapToSpawn == null)
        {
            Log.Warn($"Mukraju couldn't be spawned be spawned because the specified map hasn't been instantiated. MapId: {mapId.ToString()}");
            return;
        }

        IMonsterData data = new MonsterData(_npcMonsterManager.GetNpc(monsterVNum))
        {
            SuggestedFaction = isAngel ? FactionType.Demon : FactionType.Angel
        };

        IMonsterEntity mukraju = _monsterEntityFactory.CreateMonster(monsterVNum, data, mapToSpawn, new MonsterEntityBuilder
        {
            PositionX = mapX,
            PositionY = mapY,
            IsHostile = true,
            IsWalkingAround = true,
            IsRespawningOnDeath = false
        });

        mukraju.AddEvent(BattleTriggers.OnDeath, new Act4MukrajuDeathEvent(e.FactionType), true);
        await mukraju.EmitEventAsync(new MapJoinMonsterEntityEvent(mukraju, mapX, mapY));

        _act4Manager.RegisterMukraju(DateTime.UtcNow, mukraju, e.FactionType);

        _sessionManager.Broadcast(x =>
        {
            string faction = _languageService.GetLanguage(isAngel ? GameDialogKey.ACT4_SHOUTMESSAGE_CAMP_ANGELS : GameDialogKey.ACT4_SHOUTMESSAGE_CAMP_DEMONS, x.UserLanguage);
            return x.GenerateMsgPacket(_languageService.GetLanguageFormat(GameDialogKey.ACT4_SHOUTMESSAGE_MUKRAJU_SPAWNED, x.UserLanguage, faction), MsgMessageType.Middle);
        });
    }
}