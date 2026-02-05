using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Revival;

namespace Plugin.Alzanor.EventHandlers;

public class RevivalStartProcedureEventAlzanorMapHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly AlzanorConfiguration _alzanorConfiguration;
    private readonly IAlzanorManager _alzanorManager;

    public RevivalStartProcedureEventAlzanorMapHandler(IGameLanguageService languageService, AlzanorConfiguration alzanorConfiguration, IAlzanorManager alzanorManager)
    {
        _languageService = languageService;
        _alzanorConfiguration = alzanorConfiguration;
        _alzanorManager = alzanorManager;
    }

    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        if (e.Sender.CurrentMapInstance.MapInstanceType is not MapInstanceType.Alzanor)
        {
            return;
        }
        if (e.Sender.PlayerEntity.IsOnVehicle)
        {
            await e.Sender.EmitEventAsync(new RemoveVehicleEvent());
        }
        
        await e.Sender.PlayerEntity.RemoveBuffsOnDeathAsync();
        
        e.Sender.RefreshStat();
        DateTime actualTime = DateTime.UtcNow;
        e.Sender.PlayerEntity.UpdateRevival(actualTime + TimeSpan.FromSeconds(_alzanorConfiguration.SecondsBeingDead), RevivalType.DontPayRevival, ForcedType.Forced);
        string revivalPacket = CharacterPacketExtension.GenerateRevivalPacket(RevivalType.DontPayRevival);
        e.Sender.SendPacket(revivalPacket);
    }
}