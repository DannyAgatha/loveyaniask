using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyChangeFactionMessageConsumer : IMessageConsumer<FamilyChangeFactionMessage>
    {
        private readonly FamilyConfiguration _familyConfiguration;
        private readonly IFamilyManager _familyManager;
        private readonly SerializableGameServer _serializableGameServer;
        private readonly ISessionManager _sessionManager;

        public FamilyChangeFactionMessageConsumer(IFamilyManager familyManager, ISessionManager sessionManager, FamilyConfiguration familyConfiguration, SerializableGameServer serializableGameServer)
        {
            _familyManager = familyManager;
            _sessionManager = sessionManager;
            _familyConfiguration = familyConfiguration;
            _serializableGameServer = serializableGameServer;
        }

        public async Task HandleAsync(FamilyChangeFactionMessage notification, CancellationToken token)
        {
            long familyId = notification.FamilyId;
            FactionType factionType = notification.NewFaction;

            Family family = _familyManager.GetFamilyByFamilyId(familyId);
            if (family == null)
            {
                return;
            }

            family.Faction = (byte)notification.NewFaction;
            foreach (FamilyMembership member in family.Members.ToList())
            {
                IClientSession memberSession = _sessionManager.GetSessionByCharacterId(member.CharacterId);
                if (memberSession == null)
                {
                    continue;
                }

                if (memberSession.PlayerEntity.Faction == factionType)
                {
                    continue;
                }

                await memberSession.EmitEventAsync(new ChangeFactionEvent
                {
                    NewFaction = factionType
                });

                if (memberSession.CurrentMapInstance == null)
                {
                    continue;
                }

                if (_serializableGameServer.ChannelType == GameChannelType.ACT_4)
                {
                    if (memberSession.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
                    {
                        await memberSession.EmitEventAsync(new TimeSpaceLeavePartyEvent());
                    }

                    await memberSession.EmitEventAsync(new PlayerReturnFromAct4Event());
                    continue;
                }

                if (memberSession.CurrentMapInstance.MapInstanceType == MapInstanceType.NormalInstance)
                {
                    memberSession.ChangeToLastBaseMap();
                    continue;
                }

                await memberSession.EmitEventAsync(new PlayerReturnFromAct4Event());
            }

            FamilyPacketExtensions.SendFamilyInfoToMembers(family, _sessionManager, _familyConfiguration);
        }
    }
}