using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalStartProcedureEventPrivateMapHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly GameMinMaxConfiguration _minMaxConfiguration;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly PlayerRevivalConfiguration _revivalConfiguration;

    public RevivalStartProcedureEventPrivateMapHandler(GameRevivalConfiguration gameRevivalConfiguration, IGameLanguageService languageService, GameMinMaxConfiguration minMaxConfiguration, IRankingManager rankingManager, IReputationConfiguration reputationConfiguration)
    {
        _languageService = languageService;
        _minMaxConfiguration = minMaxConfiguration;
        _rankingManager = rankingManager;
        _reputationConfiguration = reputationConfiguration;
        _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;
    }

    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        if (e.Sender.CurrentMapInstance.MapInstanceType is not MapInstanceType.PrivateInstance)
        {
            return;
        }

        if (e.Sender.PlayerEntity.IsOnVehicle)
        {
            await e.Sender.EmitEventAsync(new RemoveVehicleEvent());
        }

        await e.Sender.PlayerEntity.RemoveBuffsOnDeathAsync();
        e.Sender.RefreshStat();
        
        PlayerRevivalPenalization playerRevivalPenalization = _revivalConfiguration.PlayerRevivalPenalization;
        if (e.Sender.PlayerEntity.Level > playerRevivalPenalization.MaxLevelWithoutRevivalPenalization)
        {
            int amount = e.Sender.PlayerEntity.Level < playerRevivalPenalization.MaxLevelWithDignityPenalizationIncrement
                ? e.Sender.PlayerEntity.Level * playerRevivalPenalization.DignityPenalizationIncrementMultiplier
                : playerRevivalPenalization.MaxLevelWithDignityPenalizationIncrement * playerRevivalPenalization.DignityPenalizationIncrementMultiplier;

            await e.Sender.PlayerEntity.RemoveDignity(amount, _minMaxConfiguration, _languageService, _reputationConfiguration, _rankingManager.TopReputation);
        }

        DateTime actualTime = DateTime.UtcNow;
        e.Sender.PlayerEntity.UpdateRevival(actualTime + _revivalConfiguration.ForcedRevivalDelay, RevivalType.DontPayRevival, ForcedType.Forced);
        e.Sender.PlayerEntity.UpdateAskRevival(actualTime + _revivalConfiguration.RevivalDialogDelay, AskRevivalType.BasicRevival);
    }
}