using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.MultiLanguage;
using WingsAPI.Communication.Families;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyLeaveEventHandler : IAsyncEventProcessor<FamilyLeaveEvent>
    {
        private readonly IFamilyService _familyService;
        private readonly IGameLanguageService _gameLanguage;

        public FamilyLeaveEventHandler(IGameLanguageService gameLanguage, IFamilyService familyService)
        {
            _gameLanguage = gameLanguage;
            _familyService = familyService;
        }

        public async Task HandleAsync(FamilyLeaveEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            if (!session.PlayerEntity.IsInFamily())
            {
                return;
            }

            if (session.PlayerEntity.IsHeadOfFamily())
            {
                session.SendInfoi2(Game18NConstString.FamilyHeadCanNotLeave);
                return;
            }

            if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
            {
                session.SendMsgi(MessageType.Default, Game18NConstString.OnlyAvailableOnGeneralMap);
                return;
            }

            await session.FamilyAddLogAsync(FamilyLogType.MemberLeave, session.PlayerEntity.Name);
            await session.EmitEventAsync(new FamilyLeftEvent
            {
                FamilyId = session.PlayerEntity.Family.Id
            });

            await _familyService.RemoveMemberToFamilyAsync(new FamilyRemoveMemberRequest
            {
                CharacterId = session.PlayerEntity.Id,
                FamilyId = session.PlayerEntity.Family.Id
            });
            session.SendGidxPacket(session.PlayerEntity.Family, _gameLanguage);
            session.SendInfoi2(Game18NConstString.LeftFamily); 
        }
    }
}