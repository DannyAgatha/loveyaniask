using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyMemberAddedMessageConsumer : IMessageConsumer<FamilyMemberAddedMessage>
    {
        private readonly FamilyConfiguration _familyConfiguration;
        private readonly IFamilyManager _familyManager;
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public FamilyMemberAddedMessageConsumer(IFamilyManager familyManager, ISessionManager sessionManager, IGameLanguageService gameLanguage, FamilyConfiguration familyConfiguration)
        {
            _familyManager = familyManager;
            _sessionManager = sessionManager;
            _gameLanguage = gameLanguage;
            _familyConfiguration = familyConfiguration;
        }

        public async Task HandleAsync(FamilyMemberAddedMessage e, CancellationToken cancellation)
        {
            long senderId = e.SenderId;
            Family family = _familyManager.GetFamilyByFamilyId(e.AddedMember.FamilyId);
            _familyManager.AddOrReplaceMember(e.AddedMember, family);
            FamilyPacketExtensions.SendFamilyMembersInfoToMembers(family, _sessionManager, _familyConfiguration);

            _sessionManager.BroadcastToFamily(e.AddedMember.FamilyId,
                async x => x.GenerateMsgPacket(_gameLanguage.GetLanguageFormat(GameDialogKey.FAMILY_SHOUTMESSAGE_MEMBER_JOINED, x.UserLanguage, e.Nickname), MsgMessageType.Middle));

            IClientSession localAddedMemberSession = _sessionManager.GetSessionByCharacterId(e.AddedMember.CharacterId);

            if (localAddedMemberSession == null)
            {
                return;
            }

            if (localAddedMemberSession.PlayerEntity.Faction != (FactionType)family.Faction)
            {
                await localAddedMemberSession.EmitEventAsync(new ChangeFactionEvent
                {
                    NewFaction = (FactionType)family.Faction
                });
            }

            localAddedMemberSession.BroadcastGidx(family, _gameLanguage);

            FamilyMembership senderMembership = family.Members.FirstOrDefault(x => x.CharacterId == senderId);
            if (senderMembership == null)
            {
                return;
            }

            await localAddedMemberSession.FamilyAddLogAsync(FamilyLogType.MemberJoin, senderMembership.Character.Name, localAddedMemberSession.PlayerEntity.Name);
        }
    }
}