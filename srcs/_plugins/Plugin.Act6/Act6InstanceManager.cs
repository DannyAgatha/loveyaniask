using PhoenixLib.Scheduler;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act6;
using WingsEmu.Game.Act6.Configuration;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act6;

public class Act6InstanceManager : IAct6InstanceManager
{
    private readonly Act6Configuration _act6Configuration;
    private readonly IMapManager _mapManager;
    private readonly IPortalFactory _portalFactory;
    private readonly IScheduler _scheduler;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISessionManager _sessionManager;
    private readonly IMonsterEntityFactory _entity;
    private readonly SerializableGameServer _serializableGameServer;
    private readonly IGameLanguageService _languageService;

    public Act6InstanceManager(Act6Configuration act6Configuration, IMapManager mapManager,
        IPortalFactory portalFactory, IScheduler scheduler, IRandomGenerator randomGenerator,
        ISessionManager sessionManager, SerializableGameServer serializableGameServer,
        IMonsterEntityFactory entity, IGameLanguageService languageService)
    {
        _act6Configuration = act6Configuration;
        _mapManager = mapManager;
        _portalFactory = portalFactory;
        _scheduler = scheduler;
        _randomGenerator = randomGenerator;
        _sessionManager = sessionManager;
        _serializableGameServer = serializableGameServer;
        _entity = entity;
        _languageService = languageService;
    }

    public Act6InstanceObject Audience { get; set; } = new();
    public Act6InstanceObject PvpInstance { get; set; } = new();

    public void DisableAudience()
    {
        Audience.InstanceActive = false;
    }

    public void EnableAudience(FactionType faction)
    {
        if (Audience.InstanceActive)
        {
            return;
        }
        Audience.InstanceActive = true;
        Audience.InstanceFaction = faction;
        DateTime currentTime = DateTime.UtcNow;
        Audience.InstanceEnd = currentTime.AddSeconds(_act6Configuration.InstanceDuration.TotalSeconds);
        Audience.InstanceStart = currentTime;

        IMapInstance portalMap = _mapManager.GetBaseMapInstanceByMapId(Audience.InstanceFaction == FactionType.Angel ? 232 : 236);
        if (portalMap == null)
        {
            return;
        }

        var portalPos = new Position((short)(Audience.InstanceFaction == FactionType.Angel ? 103 : 130),
            (short)(Audience.InstanceFaction == FactionType.Angel ? 125 : 117));
        IPortalEntity portal = _portalFactory.CreatePortal(PortalType.Raid, portalMap, portalPos, -1, portalPos);
        portal.RaidType = (short)(Audience.InstanceFaction == FactionType.Angel ? 23 : 24);
        portalMap.AddPortalToMap(portal, _scheduler, (int)_act6Configuration.InstanceDuration.TotalSeconds, true);
    }

    public void EnablePvpInstance()
    {
        PvpInstance.InstanceActive = true;
        DateTime currentTime = DateTime.UtcNow;
        PvpInstance.InstanceStart = currentTime;
        PvpInstance.InstanceEnd = currentTime.AddSeconds(_act6Configuration.PvpInstanceDuration.TotalSeconds);

        int randomMapId = _randomGenerator.RandomNumber(0, Audience.InstanceFaction == FactionType.Angel ? _angelMapId.Length : _demonMapId.Length);
        int finalMapId = Audience.InstanceFaction == FactionType.Angel ? _angelMapId[randomMapId] : _demonMapId[randomMapId];

        PvpMap = _mapManager.GetBaseMapInstanceByMapId(finalMapId);

        // in 30 sec the audience will start
        void ShoutMsg(byte duration)
        {
            _sessionManager.Broadcast(x =>
                x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.PVP_INSTANCE_ACT6_START,
                    _languageService.GetMapName(_mapManager.GetMapByMapId(PvpMap.MapId), x),
                    _serializableGameServer.ChannelId, duration), MsgMessageType.Middle));
        }

        ShoutMsg(30);
        _scheduler.Schedule(TimeSpan.FromSeconds(10), () =>
        {
            ShoutMsg(20);
        });
        _scheduler.Schedule(TimeSpan.FromSeconds(20), () =>
        {
            ShoutMsg(10);
        });
        _scheduler.Schedule(TimeSpan.FromSeconds(30), () =>
        {
            PvpInstanceStuff(Audience.InstanceFaction);
        });
    }

    public void DisablePvpInstance()
    {
        PvpInstance.InstanceActive = false;

        if (PvpMap == null)
        {
            return;
        }

        foreach (IClientSession i in PvpMap.Sessions)
        {
            i.SendPacket(i.GenerateRemoveRedClock());
        }

        var mobToRemove = new List<int> { (int)MonsterVnum.OVERSEER_AMON, (int)MonsterVnum.ARCHANGEL_LUCIFER };
        foreach (IMonsterEntity i in PvpMap.GetAliveMonsters(s => mobToRemove.Contains(s.MonsterVNum)))
        {
            PvpMap.DespawnMonster(i);
            PvpMap.RemoveMonster(i);
        }
        PvpMap.IsPvp = false;
        PvpMap.IsAct6PvpInstance = false;
        PvpMap = null;
    }

    private readonly short[] _angelMapId = [229, 230, 231, 232];
    private readonly short[] _demonMapId = [233, 234, 235, 236];

    public IMapInstance? PvpMap { get; set; }

    private void PvpInstanceStuff(FactionType faction)
    {
        if (PvpMap == null)
        {
            return;
        }

        PvpMap.IsPvp = true;
        PvpMap.IsAct6PvpInstance = true;

        foreach (IClientSession i in PvpMap.Sessions)
        {
            if (!i.PlayerEntity.IsInGroup())
            {
                continue;
            }

            i.EmitEvent(new LeaveGroupEvent());
        }

        IMapInstance cylloan = _mapManager.GetBaseMapInstanceByMapId(228);
        if (cylloan == null)
        {
            return;
        }

        var portalPos = new Position((short)(Audience.InstanceFaction == FactionType.Angel ? 122 : 67),
            (short)(Audience.InstanceFaction == FactionType.Angel ? 157 : 35));

        IPortalEntity portal = _portalFactory.CreatePortal(PortalType.TimeSpace, cylloan, portalPos, PvpMap.MapId, PvpMap.GetRandomPosition());
        cylloan.AddPortalToMap(portal, _scheduler, (int)_act6Configuration.PvpInstanceDuration.TotalSeconds, true);
        
        int monsterBossVNum = Audience.InstanceFaction == FactionType.Angel ? (int)MonsterVnum.ARCHANGEL_LUCIFER : (int)MonsterVnum.OVERSEER_AMON;
        bool monsterAlreadyExists = PvpMap.GetAliveMonsters().Any(m => m.MonsterVNum == monsterBossVNum);

        if (!monsterAlreadyExists)
        {
            Position randomPosForMonster = PvpMap.GetRandomPosition();
            IMonsterEntity monsterEntity = _entity.CreateMonster(monsterBossVNum, PvpMap, new MonsterEntityBuilder
            {
                IsWalkingAround = true,
                IsHostile = true,
                IsRespawningOnDeath = true
            });
            monsterEntity.ChangePosition(randomPosForMonster);
            monsterEntity.FirstX = randomPosForMonster.X;
            monsterEntity.FirstY = randomPosForMonster.Y;
            monsterEntity.EmitEvent(new MapJoinMonsterEntityEvent(monsterEntity));
        }

        _sessionManager.Broadcast(x =>
            x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.PVP_INSTANCE_ACT6_START_ON_MAP,
                _languageService.GetMapName(_mapManager.GetMapByMapId(PvpMap.MapId), x),
                _serializableGameServer.ChannelId), MsgMessageType.Middle));
    }
}