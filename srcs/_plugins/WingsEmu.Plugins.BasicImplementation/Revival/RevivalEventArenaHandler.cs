using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalEventArenaHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly PlayerRevivalPenalization _revivalPenalization;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public RevivalEventArenaHandler(GameRevivalConfiguration revivalConfiguration, IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _gameLanguage = gameLanguage;
        _spPartnerConfiguration = spPartnerConfiguration;
        _revivalPenalization = revivalConfiguration.PlayerRevivalConfiguration.PlayerRevivalPenalization;
    }

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        IClientSession sender = e.Sender;
        IPlayerEntity character = e.Sender.PlayerEntity;

        if (!sender.HasCurrentMapInstance)
        {
            return;
        }

        if (character.IsAlive() || character.MapInstance.MapInstanceType != MapInstanceType.ArenaInstance)
        {
            return;
        }

        character.Hp = 1;
        character.Mp = 1;

        bool hasPaidPenalization = false;
        if (e.RevivalType == RevivalType.TryPayArenaRevival && e.Forced != ForcedType.HolyRevival)
        {
            hasPaidPenalization = character.RemoveGold(_revivalPenalization.ArenaGoldPenalization);
        }

        if (e.Forced == ForcedType.HolyRevival)
        {
            hasPaidPenalization = true;
        }

        if (hasPaidPenalization)
        {
            await character.Restore(restoreMates: false);
            sender.RefreshStat();
            sender.BroadcastTeleportPacket();
            sender.BroadcastInTeamMembers(_gameLanguage, _spPartnerConfiguration);
            sender.RefreshParty(_spPartnerConfiguration);
        }
        else
        {
            await sender.Respawn();
        }

        sender.BroadcastRevive();
        sender.UpdateVisibility();
        await sender.CheckPartnerBuff();
        e.Sender.SendBuffsPacket();
        sender.PlayerEntity.ArenaImmunity = DateTime.UtcNow;
    }
}