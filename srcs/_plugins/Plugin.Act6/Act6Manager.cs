using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act6;
using WingsEmu.Game.Act6.Configuration;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act6
{
    internal class Act6Manager : IAct6Manager
    {
        private readonly Act6Configuration _act6Configuration;
        private readonly ISessionManager _sessionManager;
        private readonly IAct6InstanceManager _act6InstanceManager;
        private readonly SerializableGameServer _serializableGameServer;
        
        public Act6Manager(Act6Configuration act6Configuration, ISessionManager sessionManager, 
            IAct6InstanceManager act6InstanceManager, SerializableGameServer serializableGameServer)
        {
            _act6Configuration = act6Configuration;
            _sessionManager = sessionManager;
            _act6InstanceManager = act6InstanceManager;
            _serializableGameServer = serializableGameServer;
        }

        public Act6Faction AngelFaction { get; set; } = new();
        public Act6Faction DemonFaction { get; set; } = new();
        public bool FactionPointsLocked { get; set; }

        public void ResetFaction(FactionType factionType)
        {
            FactionPointsLocked = false;
            Act6Faction faction = factionType == FactionType.Angel ? AngelFaction : DemonFaction;
            faction.Mode = 0;
            faction.TotalTime = 0;
        }

        public void OpenEvents(FactionType factionType)
        {
            FactionPointsLocked = true;
            Act6Faction faction = factionType == FactionType.Angel ? AngelFaction : DemonFaction;
            faction.TotalTime = (short)_act6Configuration.InstanceDuration.TotalSeconds;
            faction.Mode = 1;
            faction.Points = 0;
            faction.TimeOpen = DateTime.UtcNow;
            _act6InstanceManager.EnableAudience(factionType);
        }

        public void AddFactionPoints(FactionType factionType, int amount)
        {
            if (FactionPointsLocked)
            {
                return;
            }

            Act6Faction faction = factionType == FactionType.Angel ? AngelFaction : DemonFaction;
            if (faction.Mode != 0)
            {
                return;
            }
            faction.Points += amount;
            if (faction.Points >= _act6Configuration.MaximumFactionPoints)
            {
                faction.Points = _act6Configuration.MaximumFactionPoints;
                FactionPointsLocked = true;

                if (factionType == FactionType.Angel)
                {
                    _sessionManager.Broadcast(x =>
                        x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.AUDIENCE_STARTED, 
                            "Zenas", _serializableGameServer.ChannelId), MsgMessageType.Middle));
                }
                else
                {
                    _sessionManager.Broadcast(x =>
                        x.GenerateMsgPacket(x.GetLanguageFormat(GameDialogKey.AUDIENCE_STARTED, 
                            "Erenia", _serializableGameServer.ChannelId), MsgMessageType.Middle));
                }
                OpenEvents(factionType);
            }
            Act6Status status = GetStatus();
            string packet = UiPacketExtension.GenerateAct6PacketUi(status);
            _sessionManager.Broadcast(packet, new InMapFlagBrodcast(MapFlags.ACT_6_1));
        }

        public Act6Status GetStatus()
        {
            AngelFaction.CurrentTime = AngelFaction.Mode == 0 ? 0 : (int)(AngelFaction.TimeOpen.AddSeconds(AngelFaction.TotalTime) - DateTime.Now).TotalSeconds;
            DemonFaction.CurrentTime = DemonFaction.Mode == 0 ? 0 : (int)(DemonFaction.TimeOpen.AddSeconds(DemonFaction.TotalTime) - DateTime.Now).TotalSeconds;

            return new Act6Status(
                Convert.ToByte(AngelFaction.Points / _act6Configuration.MaximumFactionPoints * 100), AngelFaction.CurrentTime, AngelFaction.TotalTime, AngelFaction.Mode,
                Convert.ToByte(DemonFaction.Points / _act6Configuration.MaximumFactionPoints * 100), DemonFaction.CurrentTime, DemonFaction.TotalTime, DemonFaction.Mode);
        }
    }
}