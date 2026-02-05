using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyChangeTitleEventHandler : IAsyncEventProcessor<FamilyChangeTitleEvent>
    {
        private readonly IFamilyService _familyService;
        private readonly IGameLanguageService _gameLanguageService;

        public FamilyChangeTitleEventHandler(IFamilyService familyService, IGameLanguageService gameLanguageService)
        {
            _familyService = familyService;
            _gameLanguageService = gameLanguageService;
        }

        public async Task HandleAsync(FamilyChangeTitleEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IFamily sessionFamily = session.PlayerEntity.Family;

            if (sessionFamily == null)
            {
                session.SendInfo(_gameLanguageService.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
                return;
            }

            if (session.PlayerEntity.GetFamilyAuthority() != FamilyAuthority.Head)
            {
                session.SendInfo(_gameLanguageService.GetLanguage(GameDialogKey.FAMILY_INFO_NOT_FAMILY_HEAD, session.UserLanguage));
                return;
            }

            FamilyMembership target = sessionFamily.Members.FirstOrDefault(x => x.Character?.Name == e.MemberNickname);
            if (target == null)
            {
                return;
            }

            await _familyService.ChangeTitleByIdAsync(new FamilyChangeTitleRequest
            {
                CharacterId = target.CharacterId,
                RequestedFamilyTitle = e.Title
            });
        }
    }
}