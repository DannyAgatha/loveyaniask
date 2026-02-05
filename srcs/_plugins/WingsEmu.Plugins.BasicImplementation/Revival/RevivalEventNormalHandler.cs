using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalEventNormalHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly IGameLanguageService _gameLanguageService;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public RevivalEventNormalHandler(IGameLanguageService gameLanguageService, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _gameLanguageService = gameLanguageService;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity == null)
        {
            return;
        }

        IPlayerEntity character = e.Sender.PlayerEntity;
        if (character.IsAlive() || character.MapInstance == null ||
            character.MapInstance.MapInstanceType != MapInstanceType.NormalInstance
            && e.Sender.PlayerEntity.MapInstance.MapInstanceType != MapInstanceType.EventGameInstance
            && character.MapInstance.MapInstanceType != MapInstanceType.Miniland)
        {
            return;
        }

        await BaseRevive(e);
    }

    public async Task BaseRevive(RevivalReviveEvent e)
    {
        IPlayerEntity character = e.Sender.PlayerEntity;
        await character.Restore(restoreMates: false);

        if (character.MapInstance.MapInstanceType != MapInstanceType.Miniland && e.RevivalType == RevivalType.DontPayRevival && e.Forced != ForcedType.HolyRevival)
        {
            await e.Sender.Respawn();
        }

        if (e.Forced == ForcedType.HolyRevival)
        {
            e.Sender.PlayerEntity.Hp = e.Sender.PlayerEntity.MaxHp;
            e.Sender.PlayerEntity.Mp = e.Sender.PlayerEntity.MaxMp;
            e.Sender.RefreshStat();
        }

        e.Sender.BroadcastRevive();
        e.Sender.UpdateVisibility();
        e.Sender.BroadcastInTeamMembers(_gameLanguageService, _spPartnerConfiguration);
        e.Sender.RefreshParty(_spPartnerConfiguration);
        await e.Sender.CheckPartnerBuff();
        e.Sender.SendBuffsPacket();
    }
}