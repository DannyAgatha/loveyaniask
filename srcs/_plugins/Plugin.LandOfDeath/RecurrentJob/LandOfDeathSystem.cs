using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.LandOfDeath.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.LandOfDeath.RecurrentJob;

public class LandOfDeathSystem : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly ILandOfDeathManager _landOfDeathManager;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly IMapManager _mapManager;
    private readonly LandOfDeathConfiguration _landOfDeathConfiguration;
    private readonly ILandOfDeathConfig _landOfDeathConfig;
    private readonly SerializableGameServer _serializableGameServer;
    private readonly ISessionManager _sessionManager;
    
    private static bool _firstShout;
    private static bool _secondShout;

    public LandOfDeathSystem(IAsyncEventPipeline eventPipeline, ILandOfDeathManager landOfDeathManager, IMonsterEntityFactory monsterEntityFactory,
        IMapManager mapManager, LandOfDeathConfiguration landOfDeathConfiguration, ILandOfDeathConfig landOfDeathConfig, SerializableGameServer serializableGameServer,
        ISessionManager sessionManager)
    {
        _eventPipeline = eventPipeline;
        _landOfDeathManager = landOfDeathManager;
        _monsterEntityFactory = monsterEntityFactory;
        _mapManager = mapManager;
        _landOfDeathConfiguration = landOfDeathConfiguration;
        _landOfDeathConfig = landOfDeathConfig;
        _serializableGameServer = serializableGameServer;
        _sessionManager = sessionManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[LAND_OF_DEATH] Start Land of Death System...");

        await ProcessCheckAndActivateEventOnStartup();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MainProcess();
            }
            catch (Exception e)
            {
                Log.Error("[LAND_OF_DEATH] ", e);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessCheckAndActivateEventOnStartup()
    {
        DateTime now = DateTime.UtcNow;
        IEnumerable<LandOfDeathTimes> openTimes = _landOfDeathConfig.GetOpenTimesByChannelId(_serializableGameServer.ChannelId);

        foreach (LandOfDeathTimes timeSlot in openTimes)
        {
            DateTime start = DateTime.UtcNow.Date.Add(timeSlot.Start);
            DateTime end = DateTime.UtcNow.Date.Add(timeSlot.End);

            if (now < start || now >= end)
            {
                continue;
            }

            Log.Info($"[LAND_OF_DEATH] Found LOD active event on startup: {start} to {end}");
            _landOfDeathManager.Start = start;
            _landOfDeathManager.End = end;
            _landOfDeathManager.IsActive = true;
            _landOfDeathManager.IsDevilActive = false;

            _sessionManager.Broadcast(x => x.GenerateMsgPacket(
                x.GetLanguageFormat(GameDialogKey.LAND_OF_DEATH_HAS_OPENED, _serializableGameServer.ChannelId),
                MsgMessageType.Middle));

            return;
        }

        Log.Info("[LAND_OF_DEATH] No active LOD events found on startup.");
    }

    private async Task MainProcess()
    {
        DateTime dateNow = DateTime.UtcNow;
        
        if (!_landOfDeathManager.IsActive)
        {
            await TryStart(dateNow);
            return;
        }

        if ((dateNow.Hour % 2) == 1)
        {
            switch (dateNow.Minute)
            {
                case 50 when !_firstShout:
                    _sessionManager.Broadcast(x =>
                        x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.LAND_OF_DEATH_WILL_OPEN_IN_MINUTES, 15,
                            _serializableGameServer.ChannelId), MsgMessageType.Middle));
                    _firstShout = true;
                    break;
                case 60 when !_secondShout:
                    _sessionManager.Broadcast(x =>
                        x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.LAND_OF_DEATH_WILL_OPEN_IN_MINUTES, 5,
                            _serializableGameServer.ChannelId), MsgMessageType.Middle));
                    _secondShout = true;
                    break;
            }
        }

        if (_landOfDeathManager.IsActive)
        {
            await ProcessDevilSpawn(dateNow);
            await ProcessDevilDespawn(dateNow);
            await Task.Delay(2000);
            await ProcessEnd(dateNow).ConfigureAwait(false);
            return;
        }

        await ProcessCheckAndActivateNextOnEnd(dateNow);
    }
    
    private async Task TryStart(DateTime now)
    {
        IEnumerable<LandOfDeathTimes> openTimes = _landOfDeathConfig.GetOpenTimesByChannelId(_serializableGameServer.ChannelId);
        if (openTimes == null)
        {
            return;
        }

        int currentHour = now.Hour;
        int currentMinute = now.Minute;

        LandOfDeathTimes openTime = openTimes.FirstOrDefault(x => x.Start.Hours == currentHour && x.Start.Minutes == currentMinute);
        if (openTime == null)
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new LandOfDeathStartEvent
        {
            Start = now,
            End = now + (openTime.End - openTime.Start)
        });
    }

    private async Task ProcessCheckAndActivateNextOnEnd(DateTime now)
    {
        IEnumerable<LandOfDeathTimes> openTimes = _landOfDeathConfig.GetOpenTimesByChannelId(_serializableGameServer.ChannelId);

        foreach (LandOfDeathTimes timeSlot in openTimes)
        {
            DateTime start = DateTime.UtcNow.Date.Add(timeSlot.Start);
            DateTime end = DateTime.UtcNow.Date.Add(timeSlot.End);

            if (now < start || now >= end || _landOfDeathManager.IsActive)
            {
                continue;
            }

            Log.Info($"[LAND_OF_DEATH] Activating LOD event for interval: {start} to {end}");
            _landOfDeathManager.Start = start;
            _landOfDeathManager.End = end;
            _landOfDeathManager.IsActive = true;
            _landOfDeathManager.IsDevilActive = false;

            _sessionManager.Broadcast(x => x.GenerateMsgPacket(
                x.GetLanguageFormat(GameDialogKey.LAND_OF_DEATH_HAS_OPENED, _serializableGameServer.ChannelId),
                MsgMessageType.Middle));

            return;
        }

        Log.Info("[LAND_OF_DEATH] No active LOD events found for the current time.");
    }


    private async Task ProcessDevilSpawn(DateTime now)
    {
        if (_landOfDeathManager.Start + _landOfDeathConfiguration.DevilStartTime >= now)
        {
            return;
        }

        if (_landOfDeathManager.NextDevilSpawn >= now)
        {
            return;
        }

        _landOfDeathManager.NextDevilSpawn = now + _landOfDeathConfiguration.DevilSpawnTime;
        _landOfDeathManager.NextDevilDespawn = now + _landOfDeathConfiguration.DevilOnMapTime;

        foreach (LandOfDeathInstance landOfDeathInstance in _landOfDeathManager.Instances)
        {
            Position position = landOfDeathInstance.MapInstance.GetRandomPosition();
            landOfDeathInstance.MapInstance.Broadcast(x => x.GenerateMsgiPacket(MessageType.Default, Game18NConstString.FlyingFireDevilAppears));
            if (landOfDeathInstance.LastPlayerId.HasValue)
            {
                IPlayerEntity playerEntity = landOfDeathInstance.MapInstance.GetCharacterById(landOfDeathInstance.LastPlayerId.Value) ??
                    landOfDeathInstance.MapInstance.Sessions.FirstOrDefault()?.PlayerEntity;
                if (playerEntity != null)
                {
                    position = playerEntity.Position;
                }
            }

            IMonsterEntity devil = _monsterEntityFactory.CreateMonster(_landOfDeathConfiguration.DevilMonsterVnum, landOfDeathInstance.MapInstance, new()
            {
                IsWalkingAround = true,
                IsHostile = true
            });
            landOfDeathInstance.DevilMonster = devil;
            await devil.EmitEventAsync(new MapJoinMonsterEntityEvent(devil, position.X, position.Y));
        }
    }

    private async Task ProcessDevilDespawn(DateTime now)
    {
        if (!_landOfDeathManager.NextDevilDespawn.HasValue)
        {
            return;
        }

        if (_landOfDeathManager.NextDevilDespawn.Value >= now)
        {
            return;
        }

        _landOfDeathManager.IsDevilActive = true;
        _landOfDeathManager.NextDevilDespawn = null;

        foreach (LandOfDeathInstance landOfDeathInstance in _landOfDeathManager.Instances)
        {
            if (landOfDeathInstance.DevilMonster == null)
            {
                continue;
            }
                
            landOfDeathInstance.MapInstance.Broadcast(x => x.GenerateMsgiPacket(MessageType.Default, Game18NConstString.FlyingFireDevilVanished));
            landOfDeathInstance.DevilMonster.MapInstance.Broadcast(landOfDeathInstance.DevilMonster.GenerateOut());
            await _eventPipeline.ProcessEventAsync(new MonsterDeathEvent(landOfDeathInstance.DevilMonster));
        }
    }

    public async Task ProcessEnd(DateTime now)
    {
        if (_landOfDeathManager.End > now)
        {
            return;
        }

        foreach (LandOfDeathInstance landOfDeathInstance in _landOfDeathManager.Instances)
        {
            if (landOfDeathInstance?.MapInstance == null)
            {
                continue;
            }
                
            landOfDeathInstance.MapInstance.Broadcast(session => session.GenerateMsgiPacket(MessageType.Default, Game18NConstString.OutOfLandOfDeath));

            foreach (IClientSession session in landOfDeathInstance.MapInstance.Sessions.ToArray())
            {
                if (!session.PlayerEntity.IsAlive())
                {
                    session.PlayerEntity.Hp = 1;
                }

                session.ChangeToLastBaseMap();
            }

            _mapManager.RemoveMapInstance(landOfDeathInstance.MapInstance.Id);
            landOfDeathInstance.MapInstance.Destroy();
        }

        _landOfDeathManager.Clear();
        _landOfDeathManager.IsActive = false;
        _landOfDeathManager.IsDevilActive = false;
        _firstShout = false;
        _secondShout = false;

        _sessionManager.Broadcast(x => x.GenerateMsgPacket(
            x.GetLanguageFormat(GameDialogKey.LAND_OF_DEATH_HAS_CLOSED, _serializableGameServer.ChannelId),
            MsgMessageType.Middle));

        Log.Info("[LAND_OF_DEATH] Event has been closed.");
    }
}