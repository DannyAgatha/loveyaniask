using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Families;
using WingsAPI.Data.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyAddMemberEventHandler : IAsyncEventProcessor<FamilyAddMemberEvent>
    {
        private readonly IFamilyManager _familyManager;
        private readonly IFamilyService _familyService;
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public FamilyAddMemberEventHandler(IGameLanguageService gameLanguage, IFamilyManager familyManager, IFamilyService familyService, ISessionManager sessionManager)
        {
            _gameLanguage = gameLanguage;
            _familyManager = familyManager;
            _familyService = familyService;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(FamilyAddMemberEvent e, CancellationToken cancellation)
        {
            IFamily family = _familyManager.GetFamilyByFamilyId(e.FamilyIdToJoin);
            if (family == null)
            {
                return;
            }

            if (family.Members.Count >= family.GetMaximumMembershipCapacity())
            {
                e.Sender.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_FULL, e.Sender.UserLanguage));
                return;
            }

            await _familyService.AddMemberToFamilyAsync(new FamilyAddMemberRequest
            {
                Member = new FamilyMembershipDto
                {
                    FamilyId = e.FamilyIdToJoin,
                    CharacterId = e.Sender.PlayerEntity.Id,
                    Authority = e.FamilyAuthority,
                    DailyMessage = null,
                    Experience = 0,
                    Title = FamilyTitle.Nothing,
                    JoinDate = DateTime.UtcNow
                },
                Nickname = e.Sender.PlayerEntity.Name,
                SenderId = e.SenderId
            });
        }
    }
}