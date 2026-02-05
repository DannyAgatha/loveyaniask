using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public sealed class PartnerKillBonusEventHandler : IAsyncEventProcessor<KillBonusEvent>
{
    private readonly IGameLanguageService _gameLanguage;

    public PartnerKillBonusEventHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public async Task HandleAsync(KillBonusEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monsterEntityToAttack = e.MonsterEntity;
        IPlayerEntity character = e.Sender.PlayerEntity;
        IClientSession session = e.Sender;

        if (monsterEntityToAttack == null || monsterEntityToAttack.IsAlive())
        {
            return;
        }

        IMateEntity partnerInTeam = character.MateComponent.GetTeamMember(s => s.MateType == MateType.Partner);

        if (character.Level < monsterEntityToAttack.Level + 15 && !monsterEntityToAttack.IsMateTrainer && !monsterEntityToAttack.IsSparringMonster)
        {
            if (partnerInTeam?.Specialist == null || partnerInTeam.Specialist.Agility == 100)
            {
                return;
            }

            partnerInTeam.Specialist.Agility = (byte)(partnerInTeam.Specialist.Agility + 2 > 100 ? 100 : partnerInTeam.Specialist.Agility + 2);
            session.SendPetInfo(partnerInTeam, _gameLanguage);

            if (partnerInTeam.Specialist.Agility != 100)
            {
                return;
            }

            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_MESSAGE_100_AGILITY, session.UserLanguage), ChatMessageColorType.Yellow);
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_MESSAGE_100_AGILITY, session.UserLanguage), MsgMessageType.Middle);

            return;
        }

        if (character.Level < monsterEntityToAttack.Level + 30 && !monsterEntityToAttack.IsMateTrainer && !monsterEntityToAttack.IsSparringMonster)
        {
            if (partnerInTeam?.Specialist == null || partnerInTeam.Specialist.Agility == 100)
            {
                return;
            }

            partnerInTeam.Specialist.Agility += 1;
            session.SendPetInfo(partnerInTeam, _gameLanguage);

            if (partnerInTeam.Specialist.Agility != 100)
            {
                return;
            }

            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_MESSAGE_100_AGILITY, session.UserLanguage), ChatMessageColorType.Yellow);
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_MESSAGE_100_AGILITY, session.UserLanguage), MsgMessageType.Middle);
        }
    }
}