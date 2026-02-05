using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl
{
    public class FamilyListMembersEventHandler : IAsyncEventProcessor<FamilyListMembersEvent>
    {
        private readonly IGameLanguageService _gameLanguageService;
        private readonly ISessionManager _sessionManager;

        public FamilyListMembersEventHandler(IGameLanguageService gameLanguageService, ISessionManager sessionManager)
        {
            _gameLanguageService = gameLanguageService;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(FamilyListMembersEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            if (!session.PlayerEntity.IsInFamily())
            {
                session.SendInfo(_gameLanguageService.GetLanguage(GameDialogKey.FAMILY_INFO_NOT_IN_FAMILY, e.Sender.UserLanguage));
                return;
            }

            IFamily family = session.PlayerEntity.Family;

            session.SendChatMessage($"== Members of {family.Name} ==", ChatMessageColorType.Yellow);
            foreach (FamilyMembership member in family.Members.OrderBy(s => s.Authority).ThenBy(s => s.Character.Name))
            {
                string className = member.Character.Class switch
                {
                    ClassType.Adventurer => _gameLanguageService.GetLanguage(GameDialogKey.CLASS_NAME_ADVENTURER, e.Sender.UserLanguage),
                    ClassType.Archer => _gameLanguageService.GetLanguage(GameDialogKey.CLASS_NAME_ARCHER, e.Sender.UserLanguage),
                    ClassType.Magician => _gameLanguageService.GetLanguage(GameDialogKey.CLASS_NAME_MAGE, e.Sender.UserLanguage),
                    ClassType.Swordman => _gameLanguageService.GetLanguage(GameDialogKey.CLASS_NAME_SWORDSMAN, e.Sender.UserLanguage),
                    ClassType.MartialArtist => _gameLanguageService.GetLanguage(GameDialogKey.CLASS_NAME_MARTIAL_ARTIST, e.Sender.UserLanguage),
                    _ => ""
                };
                string state = _sessionManager.IsOnline(member.CharacterId) ? $"ONLINE (Ch. {member.Character.ChannelId})" : "OFFLINE";
                session.SendChatMessage($"{member.Character.Name}({_gameLanguageService.GetLanguage(member.Authority.GetMemberLanguageKey(), session.UserLanguage)}) " +
                    $"- Lv. {member.Character.Level} - {className} - {state}", ChatMessageColorType.Yellow);
            }
        }
    }
}