using PhoenixLib.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Enum;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Portals;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act4;

public class Act4CaligorManager : IAct4CaligorManager
{
    private readonly Act4CaligorConfiguration _configuration;
    private readonly ISessionManager _sessionManager;
    private readonly IMapManager _mapManager;
    private readonly IPortalFactory _portalFactory;
    private readonly IScheduler _scheduler;
    private readonly IMonsterEntityFactory _entity;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IBuffFactory _buffFactory;

    public Act4CaligorManager(Act4CaligorConfiguration configuration, ISessionManager sessionManager, IMapManager mapManager, IPortalFactory portalFactory,
        IScheduler scheduler, IMonsterEntityFactory monsterEntityFactory, IGameItemInstanceFactory gameItemInstanceFactory, IBuffFactory buffFactory)
    {
        _configuration = configuration;
        _sessionManager = sessionManager;
        _mapManager = mapManager;
        _portalFactory = portalFactory;
        _scheduler = scheduler;
        _entity = monsterEntityFactory;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _buffFactory = buffFactory;
    }
        
    private readonly Dictionary<int, FactionType> _initialFactions = new();
        
    private readonly HashSet<int> _playersWhoLeftCaligor = [];
    public int AngelCount { get; set; }
    public int DemonCount { get; set; }
    public DateTime CaligorStart { get; set; }
    public DateTime CaligorEnd { get; set; }
    public bool CaligorActive { get; set; }
    public bool CaligorLocked { get; set; }
    public int AngelDamage { get; set; }
    public int DemonDamage { get; set; }
    public bool FernonMapsActive { get; set; }

    public void TeleportPlayerToCaligorCamp(IPlayerEntity player)
    {
        short x = 0;
        short y = 0;

        switch (player.CaligorFaction)
        {
            case FactionType.Angel:
                x = 50;
                y = 172;
                break;

            case FactionType.Demon:
                x = 130;
                y = 172;
                break;
        }
            
        player.Session.ChangeMap(CaligorInstance, x, y);
    }
        
    public FactionType GetInitialFaction(IPlayerEntity player) => _initialFactions.TryGetValue(player.Id, out FactionType faction) ? faction : player.Faction;

    public bool HasLeftCaligor(IPlayerEntity player) => _playersWhoLeftCaligor.Contains(player.Id);

    public void MarkPlayerAsLeftCaligor(IPlayerEntity player)
    {
        _playersWhoLeftCaligor.Add(player.Id);
    }

    public void EndCaligorInstance(bool caligorArekilled)
    {
        if (caligorArekilled)
        {
            FactionType winningFaction = AngelDamage > DemonDamage ? FactionType.Angel : FactionType.Demon;

            DateTime currentTime = DateTime.UtcNow;
            TimeSpan timeLeft = CaligorEnd - currentTime;

            foreach (IClientSession sess in CaligorInstance.Sessions.ToList())
            {
                var listItemRewards = new List<GameItemInstance>
                {
                    _gameItemInstanceFactory.CreateItem(5959),
                };
                
                if (timeLeft.TotalSeconds > 2400)
                {
                    listItemRewards.Add(sess.PlayerEntity.Faction == winningFaction ? _gameItemInstanceFactory.CreateItem(25414) : _gameItemInstanceFactory.CreateItem(25413));
                }
                else
                {
                    listItemRewards.Add(sess.PlayerEntity.Faction == winningFaction ? _gameItemInstanceFactory.CreateItem(25415) : _gameItemInstanceFactory.CreateItem(25413));
                }
                
                foreach (GameItemInstance rewards in listItemRewards)
                {
                    sess.AddNewItemToInventory(rewards, true, ChatMessageColorType.Yellow, true).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                
                if (sess.PlayerEntity.Family != null)
                {
                    sess.EmitEvent(new FamilyAddExperienceEvent(1000, FamXpObtainedFromType.Raid));
                }
                
                sess.EmitEvent(new IncreaseBattlePassObjectiveEvent(MissionType.CompleteXCaligor));
            }
        }
        
        _scheduler.Schedule(TimeSpan.FromSeconds(30), () =>
        {
            foreach (IClientSession sess in CaligorInstance.Sessions.ToList())
            {
                sess.ChangeMap(UnknowLandInstance.Id, sess.PlayerEntity.MapX, sess.PlayerEntity.MapY);
            }

            CaligorInstance.Destroy();
            CaligorInstance = null;
        });
        
        _sessionManager.Sessions
            .Where(session => session.CurrentMapInstance?.Id == CaligorInstance?.Id && session.PlayerEntity.CaligorFaction != null)
            .ToList()
            .ForEach(session =>
            {
                FactionType initialFaction = GetInitialFaction(session.PlayerEntity);

                session.EmitEvent(new ChangeFactionEvent
                {
                    NewFaction = initialFaction
                });

                session.RefreshFaction();

                short x = initialFaction == FactionType.Angel ? (short)70 : (short)110;
                short y = 159;

                session.ChangeMap((short)MapIds.UNKNOWN_LAND, x, y);

                session.PlayerEntity.CaligorFaction = null;
            });

        AngelCount = 0;
        DemonCount = 0;
        _initialFactions.Clear();
        _playersWhoLeftCaligor.Clear();

        _sessionManager.Broadcast(x => x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_END), MsgMessageType.Middle));
        
        foreach (IPortalEntity p in UnknowLandInstance.Portals.Where(s => s.DestinationMapInstance != null && s.DestinationMapInstance.Id == CaligorInstance.Id).ToList())
        {
            p.IsDisabled = true;
            UnknowLandInstance.Broadcast(p.GenerateGp());
            UnknowLandInstance.DeletePortal(p);
        }

        CaligorActive = false;
        AngelDamage = 0;
        DemonDamage = 0;
        
        // FernonMap = _mapManager.GenerateMapInstanceByMapId((short)MapIds.AIRSHIP_CYLLOAN, MapInstanceType.RaidInstance);
        // IPortalEntity fernonMapPortal = _portalFactory.CreatePortal(PortalType.Raid, FernonMap, new Position(145, 198), mapDestId: -1, destPos: new Position(), (short)RaidType.Fernon, -1, null, null);
        // FernonMap.AddPortalToMap(fernonMapPortal);
        //     
        // _sessionManager.Broadcast(x => x.GenerateMsgPacket(
        //     x.GetLanguageFormat(GameDialogKey.ACT4_FERNON_START), MsgMessageType.Middle));
        //     
        // IPortalEntity portal = _portalFactory.CreatePortal(PortalType.TSNormal, UnknowLandInstance, new Position(93, 93), FernonMap, new Position(145, 242));
        // UnknowLandInstance.AddPortalToMap(portal, _scheduler, (int)_configuration.FernonDurationRaidInstance.TotalSeconds, true);
        //
        // FernonMapsActive = true;
        //     
        // _scheduler.Schedule(_configuration.FernonDurationRaidInstance, () =>
        // {
        //     _sessionManager.Broadcast(x => x.GenerateMsgPacket(
        //         x.GetLanguageFormat(GameDialogKey.ACT4_FERNON_END), MsgMessageType.Middle));
        //         
        //     foreach (IClientSession sess in FernonMap.Sessions.ToList())
        //     {
        //         sess.ChangeMap(UnknowLandInstance, 93, 93);
        //     }
        //         
        //     FernonMap?.Destroy();
        //     FernonMapsActive = false;
        //     FernonMap = null;
        // });
        //     
        // void ShootFernonEnd(double min)
        // {
        //     _scheduler.Schedule(_configuration.FernonDurationRaidInstance - TimeSpan.FromMinutes(min), () =>
        //     {
        //         _sessionManager.Broadcast(x => x.GenerateMsgPacket(
        //             x.GetLanguageFormat(GameDialogKey.ACT4_FERNON_END_IN, min), MsgMessageType.Middle));
        //     });
        // }
        //     
        // ShootFernonEnd(10);
        // ShootFernonEnd(5);
        // ShootFernonEnd(4);
        // ShootFernonEnd(3);
        // ShootFernonEnd(2);
        // ShootFernonEnd(1);
    }

    private void DelayedCaligor()
    {
        _sessionManager.Broadcast(x => x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_START), MsgMessageType.Middle));

        CaligorInstance = _mapManager.GenerateMapInstanceByMapId((short)MapIds.ACT4_CALIGOR, MapInstanceType.Caligor);
        UnknowLandInstance = _mapManager.GetBaseMapInstanceByMapId((short)MapIds.UNKNOWN_LAND);
            
        IPortalEntity portal = _portalFactory.CreatePortal(PortalType.MapPortal, CaligorInstance, new Position(89, 23), UnknowLandInstance.Id, new Position(91, 35));
        CaligorInstance.AddPortalToMap(portal);

        portal = _portalFactory.CreatePortal(PortalType.MapPortal, CaligorInstance, new Position(128, 175), UnknowLandInstance.Id, new Position(110, 159));
        CaligorInstance.AddPortalToMap(portal);

        portal = _portalFactory.CreatePortal(PortalType.MapPortal, CaligorInstance, new Position(50, 174), UnknowLandInstance.Id, new Position(70, 159));
        CaligorInstance.AddPortalToMap(portal);

        portal = _portalFactory.CreatePortal(PortalType.MapPortal, UnknowLandInstance, new Position(70, 159), CaligorInstance.Id, new Position(50, 174));
        UnknowLandInstance.AddPortalToMap(portal);

        portal = _portalFactory.CreatePortal(PortalType.MapPortal, UnknowLandInstance, new Position(110, 159), CaligorInstance.Id, new Position(128, 175));
        UnknowLandInstance.AddPortalToMap(portal);

        portal = _portalFactory.CreatePortal(PortalType.MapPortal, UnknowLandInstance, new Position(91, 35), CaligorInstance.Id, new Position(89, 23));
        UnknowLandInstance.AddPortalToMap(portal);

        IMonsterEntity monsterEntity = _entity.CreateMonster((int)MonsterVnum.ALZANOR_BOSS, CaligorInstance, new MonsterEntityBuilder
        {
            IsWalkingAround = true,
            IsHostile = true,
            IsBoss = true,
            IsTarget = true,
            IsRespawningOnDeath = false,
            PositionX = 70,
            PositionY = 90,
            Direction = 2
        });
        monsterEntity.ChangePosition(new Position(70, 90));
        monsterEntity.FirstX = 70;
        monsterEntity.FirstY = 90;
        monsterEntity.EmitEvent(new MapJoinMonsterEntityEvent(monsterEntity));

        CaligorInstance.IsPvp = true;

        CaligorStart = DateTime.UtcNow;
        CaligorEnd = CaligorStart + _configuration.CaligorDuration;
        CaligorActive = true;

        RefreshCaligorInstance();
    }

    public void InitializeAndStartCaligorInstance()
    {
        _playersWhoLeftCaligor.Clear();
    
        AngelCount = 0;
        DemonCount = 0;
    
        DelayedCaligor();
    }
        
    public void UpdateFactionCount(FactionType faction)
    {
        switch (faction)
        {
            case FactionType.Angel:
                AngelCount++;
                break;
            case FactionType.Demon:
                DemonCount++;
                break;
        }
    }
        
    public void DecreaseFactionCount(FactionType faction)
    {
        switch (faction)
        {
            case FactionType.Angel:
                AngelCount = Math.Max(0, AngelCount - 1);
                break;
            case FactionType.Demon:
                DemonCount = Math.Max(0, DemonCount - 1);
                break;
        }
    }
        
    private IMapInstance CaligorInstance { get; set; }
    private IMapInstance UnknowLandInstance { get; set; }
    public IMapInstance FernonMap { get; private set; }

    public void LockCaligorInstance()
    {
        foreach (IPortalEntity p in UnknowLandInstance.Portals.Where(s => s.DestinationMapInstance != null && s.DestinationMapInstance.Id == CaligorInstance.Id).ToList())
        {
            p.IsDisabled = true;
            UnknowLandInstance.Broadcast(p.GenerateGp());
            p.IsDisabled = false;
            p.Type = PortalType.Closed;
            UnknowLandInstance.Broadcast(p.GenerateGp());
        }
        _sessionManager.Broadcast(x => x.GenerateMsgPacket(
            x.GetLanguageFormat(GameDialogKey.ACT4_CALIGOR_LOCKED), MsgMessageType.Middle));
        CaligorLocked = true;
    }

    public void RefreshCaligorInstance()
    {
        DateTime currentTime = DateTime.UtcNow;

        TimeSpan timeLeft = CaligorEnd - currentTime;

        const int maxHp = 29086082;

        if (maxHp / 10 * 8 < AngelDamage + DemonDamage && !CaligorLocked)
        {
            LockCaligorInstance();
        }
    }
}