using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl.NpcDialogs
{
    public class CreateFamilyHandler : INpcDialogAsyncHandler
    {
        private readonly IGameLanguageService _langService;

        public CreateFamilyHandler(IGameLanguageService langService) => _langService = langService;

        public NpcRunType[] NpcRunTypes => new[] { NpcRunType.FAMILY_DIALOGUE };

        public async Task Execute(IClientSession session, NpcDialogEvent e)
        {
            if (session.CantPerformActionOnAct4())
            {
                return;
            }

            if (e.Argument == 0)
            {
                if (!session.PlayerEntity.IsInGroup())
                {
                    session.SendInfo(_langService.GetLanguage(GameDialogKey.FAMILY_INFO_CREATION_GROUP_REQUIRED, session.UserLanguage));
                    return;
                }

                PlayerGroup group = session.PlayerEntity.GetGroup();
                if (group.Members.Count != 3)
                {
                    session.SendInfo(_langService.GetLanguage(GameDialogKey.FAMILY_INFO_GROUP_NOT_FULL, session.UserLanguage));
                    return;
                }

                if (group.Members.Any(s => s.Family != null))
                {
                    session.SendInfo(_langService.GetLanguage(GameDialogKey.FAMILY_INFO_GROUP_MEMBER_ALREADY_IN_FAMILY, session.UserLanguage));
                    return;
                }

                session.SendInboxPacket("#glmk^ 14 1 191"); // fuck gf i18n :pepega:
                return;
            }

            if (!session.PlayerEntity.IsInFamily())
            {
                session.SendInfo(_langService.GetLanguage(GameDialogKey.FAMILY_INFO_NOT_IN_FAMILY, session.UserLanguage));
                return;
            }

            if (session.PlayerEntity.GetFamilyAuthority() != FamilyAuthority.Head)
            {
                session.SendInfo(_langService.GetLanguage(GameDialogKey.FAMILY_INFO_NOT_FAMILY_HEAD, session.UserLanguage));
                return;
            }

            session.SendQnaPacket("glrm 1", _langService.GetLanguage(GameDialogKey.FAMILY_DIALOG_ASK_DISMISS_FAMILY, session.UserLanguage));
        }
    }
}