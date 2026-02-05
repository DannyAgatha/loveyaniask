using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;
using WingsEmu.Game.Maps;

namespace Plugin.Alzanor.EventHandlers;

public class RevivalEventAlzanorHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartner;
    private readonly AlzanorConfiguration _alzanorConfiguration;

    public RevivalEventAlzanorHandler(IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartner, AlzanorConfiguration alzanorConfiguration)
    {
        _gameLanguage = gameLanguage;
        _spPartner = spPartner;
        _alzanorConfiguration = alzanorConfiguration;
    }

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (session.CurrentMapInstance is not { MapInstanceType: MapInstanceType.Alzanor })
        {
            return;
        }
        
        if (session.PlayerEntity.IsAlive())
        {
            return;
        }

        AlzanorTeamType team = session.PlayerEntity.AlzanorComponent.Team;
        Position pos = team switch
        {
            AlzanorTeamType.Red => new Position(_alzanorConfiguration.RedStartX, _alzanorConfiguration.RedStartY),
            AlzanorTeamType.Blue => new Position(_alzanorConfiguration.BlueStartX, _alzanorConfiguration.BlueStartY),
            _ => new Position(100, 100)
        };
        
        e.Sender.PlayerEntity.TeleportOnMap(pos.X, pos.Y, true);
        e.Sender.PlayerEntity.ArenaImmunity = DateTime.UtcNow;
        e.Sender.UpdateVisibility();
        await e.Sender.PlayerEntity.Restore(restoreMates: false);
        e.Sender.BroadcastRevive();
        e.Sender.BroadcastInTeamMembers(_gameLanguage, _spPartner);
        e.Sender.RefreshParty(_spPartner);
        e.Sender.SendBuffsPacket();
    }
}