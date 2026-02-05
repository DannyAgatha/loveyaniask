using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Families;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyRemoveMemberEventHandler : IAsyncEventProcessor<FamilyRemoveMemberEvent>
    {
        private readonly IFamilyService _familyService;
        private readonly IGameLanguageService _gameLanguage;

        public FamilyRemoveMemberEventHandler(IGameLanguageService gameLanguage, IFamilyService familyService)
        {
            _gameLanguage = gameLanguage;
            _familyService = familyService;
        }

        public async Task HandleAsync(FamilyRemoveMemberEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IFamily family = session.PlayerEntity.Family;

            if (family == null)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
                return;
            }

            if (session.PlayerEntity.Name == e.Nickname)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_DIALOG_CANNOT_KICK_YOURSELF, session.UserLanguage));
                return;
            }

            FamilyAuthority sessionAuthority = session.PlayerEntity.GetFamilyAuthority();

            if (!session.PlayerEntity.IsHeadOfFamily() && sessionAuthority != FamilyAuthority.Deputy)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                return;
            }

            FamilyMembership target = family.Members.FirstOrDefault(x => x.Character?.Name == e.Nickname);
            if (target == null)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_USER_NOT_FOUND, session.UserLanguage));
                return;
            }

            if ((int)target.Authority <= (int)sessionAuthority)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                return;
            }

            await session.FamilyAddLogAsync(FamilyLogType.MemberLeave, e.Nickname);

            await _familyService.RemoveMemberToFamilyAsync(new FamilyRemoveMemberRequest
            {
                CharacterId = target.CharacterId,
                FamilyId = target.FamilyId
            });

            await session.EmitEventAsync(new FamilyKickedMemberEvent
            {
                KickedMemberId = target.CharacterId
            });
        }
    }
}