using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DiscordNotifier;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Portals;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act4.Event;

public class Act4DungeonSystemStartEventHandler : IAsyncEventProcessor<Act4DungeonSystemStartEvent>
{
    private static readonly List<DungeonType> PossibleAct4DungeonTypes = new();
    private readonly IAct4DungeonManager _act4DungeonManager;
    private readonly Act4DungeonsConfiguration _act4DungeonsConfiguration;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMapManager _mapManager;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IPortalFactory _portalFactory;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISessionManager _sessionManager;
    static Act4DungeonSystemStartEventHandler()
    {
        PossibleAct4DungeonTypes.Add(DungeonType.Berios);
        PossibleAct4DungeonTypes.Add(DungeonType.Calvinas);
        PossibleAct4DungeonTypes.Add(DungeonType.Hatus);
        PossibleAct4DungeonTypes.Add(DungeonType.Morcos);
    }

    public Act4DungeonSystemStartEventHandler(IAct4DungeonManager act4DungeonManager, IRandomGenerator randomGenerator, IMapManager mapManager, IAsyncEventPipeline asyncEventPipeline,
        Act4DungeonsConfiguration act4DungeonsConfiguration, IMonsterEntityFactory monsterEntityFactory, INpcMonsterManager npcMonsterManager, ISessionManager sessionManager,
        IGameLanguageService gameLanguage, IPortalFactory portalFactory)
    {
        _act4DungeonManager = act4DungeonManager;
        _randomGenerator = randomGenerator;
        _mapManager = mapManager;
        _asyncEventPipeline = asyncEventPipeline;
        _act4DungeonsConfiguration = act4DungeonsConfiguration;
        _monsterEntityFactory = monsterEntityFactory;
        _npcMonsterManager = npcMonsterManager;
        _sessionManager = sessionManager;
        _gameLanguage = gameLanguage;
        _portalFactory = portalFactory;
    }

    public async Task HandleAsync(Act4DungeonSystemStartEvent e, CancellationToken cancellation)
    {
        if (_act4DungeonManager.DungeonsActive)
        {
            Log.Debug("[ACT4_DUNGEON_SYSTEM] Dungeons are already started, denying new start.");
            return;
        }

        IMapInstance portalMap = _mapManager.GetBaseMapInstanceByMapId(_act4DungeonsConfiguration.DungeonPortalMapId);
        if (portalMap == null)
        {
            Log.Warn("[ACT4_DUNGEON_SYSTEM] Can't start system. The mapId defined for the dungeon portal in the configuration couldn't be obtained. " +
                $"Defined MapId: '{_act4DungeonsConfiguration.DungeonPortalMapId.ToString()}'");
            return;
        }

        (FactionType factionType, DungeonType? dungeonType) = e;
        DungeonType randomDungeonType = dungeonType ?? PossibleAct4DungeonTypes[_randomGenerator.RandomNumber(0, PossibleAct4DungeonTypes.Count)];
        _act4DungeonManager.EnableDungeons(randomDungeonType, factionType);

        var portalPos = new Position(_act4DungeonsConfiguration.DungeonPortalMapX, _act4DungeonsConfiguration.DungeonPortalMapY);
        IPortalEntity portal = _portalFactory.CreatePortal(factionType == FactionType.Angel ? PortalType.AngelRaid : PortalType.DemonRaid, portalMap, portalPos, -1, portalPos);

        await _asyncEventPipeline.ProcessEventAsync(new SpawnPortalEvent(portalMap, portal), cancellation);

        List<GuardianSpawn> guardians = factionType == FactionType.Angel ? _act4DungeonsConfiguration.GuardiansForAngels : _act4DungeonsConfiguration.GuardiansForDemons;
        var spawnedGuardians = new List<IMonsterEntity>();
        foreach (GuardianSpawn guardian in guardians)
        {
            IMonsterData npcMonster = _npcMonsterManager.GetNpc(guardian.MonsterVnum);
            if (npcMonster == null)
            {
                Log.Warn($"[ACT4_DUNGEON_SYSTEM] Couldn't spawn guardian with VNum: '{guardian.MonsterVnum.ToString()}'");
                continue;
            }

            IMonsterEntity monster = _monsterEntityFactory.CreateMonster(npcMonster, portalMap, new MonsterEntityBuilder
            {
                Direction = guardian.Direction,
                IsHostile = true
            });

            await monster.EmitEventAsync(new MapJoinMonsterEntityEvent(monster, guardian.MapX, guardian.MapY));

            spawnedGuardians.Add(monster);
        }

        _act4DungeonManager.SetGuardiansAndPortal(spawnedGuardians, portal);
        _sessionManager.Broadcast(x => { return x.GenerateMsgPacket(_gameLanguage.GetLanguage(GameDialogKey.ACT4_DUNGEON_SHOUTMESSAGE_STARTED, x.UserLanguage), MsgMessageType.Middle); },
            new FactionBroadcast(factionType));

        await _asyncEventPipeline.ProcessEventAsync(new Act4SystemFcBroadcastEvent(), cancellation);
    }
}