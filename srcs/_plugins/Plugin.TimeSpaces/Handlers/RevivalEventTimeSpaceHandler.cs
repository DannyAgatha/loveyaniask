using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace Plugin.TimeSpaces.Handlers;

public class RevivalEventTimeSpaceHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartner;

    public RevivalEventTimeSpaceHandler(IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartner)
    {
        _gameLanguage = gameLanguage;
        _spPartner = spPartner;
    }

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (session.CurrentMapInstance is not { MapInstanceType: MapInstanceType.TimeSpaceInstance })
        {
            return;
        }

        if (session.PlayerEntity.IsAlive())
        {
            return;
        }

        e.Sender.UpdateVisibility();
        await e.Sender.PlayerEntity.Restore(restoreMates: false);
        e.Sender.BroadcastRevive();
        e.Sender.BroadcastInTeamMembers(_gameLanguage, _spPartner);
        e.Sender.RefreshParty(_spPartner);
        await e.Sender.CheckPartnerBuff();
        e.Sender.SendBuffsPacket();
    }
}