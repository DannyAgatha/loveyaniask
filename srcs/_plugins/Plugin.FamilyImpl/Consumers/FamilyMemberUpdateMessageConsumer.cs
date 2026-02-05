using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyMemberUpdateMessageConsumer : IMessageConsumer<FamilyMemberUpdateMessage>
    {
        private readonly FamilyConfiguration _familyConfiguration;
        private readonly IFamilyManager _familyManager;
        private readonly IGameLanguageService _languageService;
        private readonly ISessionManager _sessionManager;

        public FamilyMemberUpdateMessageConsumer(ISessionManager sessionManager, IGameLanguageService languageService, IFamilyManager familyManager, FamilyConfiguration familyConfiguration)
        {
            _sessionManager = sessionManager;
            _languageService = languageService;
            _familyManager = familyManager;
            _familyConfiguration = familyConfiguration;
        }

        public async Task HandleAsync(FamilyMemberUpdateMessage notification, CancellationToken token)
        {
            var families = new HashSet<Family>();

            foreach (FamilyMembershipDto member in notification.UpdatedMembers)
            {
                Family family = _familyManager.GetFamilyByFamilyId(member.FamilyId);
                _familyManager.AddOrReplaceMember(member, family);
                families.Add(family);

                switch (notification.ChangedInfoMemberUpdate)
                {
                    case ChangedInfoMemberUpdate.Authority:
                        if (member.Authority == FamilyAuthority.Head)
                        {
                            _sessionManager.BroadcastToFamily(member.FamilyId,
                                async x => x.GenerateMsgPacket(_languageService.GetLanguage(GameDialogKey.FAMILY_SHOUTMESSAGE_CHANGED_HEAD, x.UserLanguage), MsgMessageType.Middle));
                        }

                        IClientSession localSession = _sessionManager.GetSessionByCharacterId(member.CharacterId);
                        localSession?.BroadcastGidx(family, _languageService);
                        break;
                }
            }

            foreach (Family family in families)
            {
                switch (notification.ChangedInfoMemberUpdate)
                {
                    case ChangedInfoMemberUpdate.Authority:
                        FamilyPacketExtensions.SendFamilyMembersAuthorityToMembers(family, _sessionManager, _familyConfiguration);
                        break;
                    case ChangedInfoMemberUpdate.Experience:
                        FamilyPacketExtensions.SendMembersExpToMembers(family, _sessionManager);
                        break;
                    case ChangedInfoMemberUpdate.DailyMessage:
                        FamilyPacketExtensions.SendMembersDailyMessages(family, _sessionManager);
                        break;
                }
            }
        }
    }
}