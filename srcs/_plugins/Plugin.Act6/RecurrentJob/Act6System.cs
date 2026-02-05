using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act6;
using WingsEmu.Game.Act6.Configuration;
using WingsEmu.Game.Act6.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act6.RecurrentJob
{
    public class Act6System : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

        private readonly IAct6Manager _act6Manager;
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly ISessionManager _sessionManager;
        private readonly IAct6InstanceManager _act6InstanceManager;
        private readonly SerializableGameServer _serializableGameServer;
        private readonly IMapManager _mapManager;
        private readonly IPortalFactory _portalFactory;
        private readonly Act6Configuration _act6Configuration;
        
        public Act6System(IAct6Manager act6Manager, ISessionManager sessionManager, 
            IAsyncEventPipeline asyncEventPipeline, IAct6InstanceManager act6InstanceManager, 
            SerializableGameServer serializableGameServer, IPortalFactory portalFactory, IMapManager mapManager,
            Act6Configuration act6Configuration)
        {
            _act6Manager = act6Manager;
            _sessionManager = sessionManager;
            _asyncEventPipeline = asyncEventPipeline;
            _act6InstanceManager = act6InstanceManager;
            _serializableGameServer = serializableGameServer;
            _portalFactory = portalFactory;
            _mapManager = mapManager;
            _act6Configuration = act6Configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Info("[ACT6_SYSTEM] Started!");
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessAct6Instance(stoppingToken);
                await ProcessAct6PvpInstance(stoppingToken);
                await _asyncEventPipeline.ProcessEventAsync(new Act6SystemBroadcastEvent(), stoppingToken);
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task ProcessAct6Instance(CancellationToken stoppingToken)
        {
            if (!_act6InstanceManager.Audience.InstanceActive)
            {
                return;
            }
            DateTime currentTime = DateTime.UtcNow;

            if (_act6InstanceManager.Audience.InstanceStart.AddMinutes(
                (_act6Configuration.InstanceDuration - _act6Configuration.PvpInstanceDuration).TotalMinutes) <= currentTime &&
                !_act6InstanceManager.PvpInstance.InstanceActive)
            {
                await _asyncEventPipeline.ProcessEventAsync(new StartPvpInstanceEvent(), stoppingToken);
            }

            if (_act6InstanceManager.Audience.InstanceEnd >= currentTime)
            {
                return;
            }
           
            _act6InstanceManager.DisableAudience();
            _sessionManager.Broadcast(x =>
                        x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.AUDIENCE_ENDED,
                        _act6InstanceManager.Audience.InstanceFaction, _serializableGameServer.ChannelId), MsgMessageType.Middle));
            _act6Manager.ResetFaction(_act6InstanceManager.Audience.InstanceFaction);
        }

        private async Task ProcessAct6PvpInstance(CancellationToken stoppingToken)
        {
            if (!_act6InstanceManager.PvpInstance.InstanceActive)
            {
                return;
            }
            DateTime currentTime = DateTime.UtcNow;

            TimeSpan timeLeft = _act6InstanceManager.PvpInstance.InstanceEnd - currentTime;
            if (_act6InstanceManager.PvpMap != null)
            {
                foreach (IClientSession sessionOnMap in _act6InstanceManager.PvpMap.Sessions)
                {
                    sessionOnMap.SendClockPacket(ClockType.RedMiddle, 0, timeLeft, _act6Configuration.PvpInstanceDuration);
                }
            }

            if (_act6InstanceManager.PvpInstance.InstanceEnd >= currentTime)
            {
                return;
            }
            
            _sessionManager.Broadcast(x =>
                x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.PVP_INSTANCE_ACT6_END,
                    _act6InstanceManager.Audience.InstanceFaction, _serializableGameServer.ChannelId), MsgMessageType.Middle));
            _act6InstanceManager.DisablePvpInstance();
        }
    }
}