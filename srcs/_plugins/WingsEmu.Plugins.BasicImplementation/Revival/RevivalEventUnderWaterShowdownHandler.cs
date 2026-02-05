using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalEventUnderwaterShowdownHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;
    
    public RevivalEventUnderwaterShowdownHandler(IGameLanguageService languageService, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _languageService = languageService;
        _spPartnerConfiguration = spPartnerConfiguration;
    }
    
    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        if (e.Sender?.CurrentMapInstance?.MapInstanceType != MapInstanceType.UnderWaterShowdown)
        {
            return;
        }

        IPlayerEntity character = e.Sender.PlayerEntity;
        if (character == null)
        {
            return;
        }

        await character.Restore(restoreMates: false);

        if (e.Sender != null)
        {
            await e.Sender.Respawn();
            e.Sender.BroadcastRevive();
            e.Sender.UpdateVisibility();
            e.Sender.BroadcastInTeamMembers(_languageService, _spPartnerConfiguration);
            e.Sender.RefreshParty(_spPartnerConfiguration);
            await e.Sender.CheckPartnerBuff();
            e.Sender.SendBuffsPacket();
        }
    }
}