using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class PlayerRestEventHandler : IAsyncEventProcessor<PlayerRestEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMeditationManager _meditation;

    public PlayerRestEventHandler(IGameLanguageService gameLanguage, IMeditationManager meditation)
    {
        _gameLanguage = gameLanguage;
        _meditation = meditation;
    }

    public async Task HandleAsync(PlayerRestEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IPlayerEntity character = e.Sender.PlayerEntity;
        bool force = e.Force;

        DateTime now = DateTime.UtcNow;
        if (!character.IsSitting &&
            (character.LastSkillUse.AddSeconds(4) > now || character.LastDefence.AddSeconds(4) > now) && !force)
        {
            Log.Debug($"{nameof(PlayerRestEventHandler)} Can't rest (probably in combat).");
            return;
        }

        if (character.IsOnVehicle || character.IsMorphed)
        {
            string message = _gameLanguage.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_IMPOSSIBLE_TO_USE, session.UserLanguage);
            session.SendChatMessage(message, ChatMessageColorType.Yellow);
            return;
        }

        character.LastSitting = now;
        character.IsSitting = !character.IsSitting;
        session.BroadcastRest();
        session.PlayerEntity.RemoveMeditation(_meditation);

        if (e.RestTeamMemberMates)
        {
            foreach (IMateEntity mate in character.MateComponent.TeamMembers())
            {
                await session.EmitEventAsync(new MateRestEvent
                {
                    MateEntity = mate,
                    Rest = character.IsSitting
                });
            }
        }

        if (!character.IsSitting)
        {
            character.ClearFoodBuffer();
        }
    }
}