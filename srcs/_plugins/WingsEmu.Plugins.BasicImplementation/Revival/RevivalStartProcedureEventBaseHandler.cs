using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalStartProcedureEventBaseHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly GameMinMaxConfiguration _minMaxConfiguration;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly PlayerRevivalConfiguration _revivalConfiguration;

    public RevivalStartProcedureEventBaseHandler(GameMinMaxConfiguration minMaxConfiguration, IGameLanguageService languageService, GameRevivalConfiguration gameRevivalConfiguration,
        IReputationConfiguration reputationConfiguration, IRankingManager rankingManager)
    {
        _minMaxConfiguration = minMaxConfiguration;
        _languageService = languageService;
        _reputationConfiguration = reputationConfiguration;
        _rankingManager = rankingManager;
        _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;
    }

    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive())
        {
            return;
        }

        if (!e.Sender.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            return;
        }

        if (e.Sender.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return;
        }

        await BaseMapDeathPenalization(e);
        AskBaseMapRevival(e);
    }

    private void AskBaseMapRevival(RevivalStartProcedureEvent e)
    {
        DateTime actualTime = DateTime.UtcNow;

        if (e.Sender.PlayerEntity.MapId == (int)MapIds.NOSVILLE)
        {
            e.Sender.PlayerEntity.UpdateRevival(actualTime + _revivalConfiguration.RevivalDialogDelay, RevivalType.DontPayRevival, ForcedType.Forced);
            return;
        }

        e.Sender.PlayerEntity.UpdateRevival(actualTime + _revivalConfiguration.ForcedRevivalDelay, RevivalType.DontPayRevival, ForcedType.Forced);
        e.Sender.PlayerEntity.UpdateAskRevival(actualTime + _revivalConfiguration.RevivalDialogDelay, AskRevivalType.BasicRevival);
    }

    private async Task BaseMapDeathPenalization(RevivalStartProcedureEvent e)
    {
        if (e.Sender.PlayerEntity.IsOnVehicle)
        {
            await e.Sender.EmitEventAsync(new RemoveVehicleEvent());
        }

        await e.Sender.PlayerEntity.RemoveBuffsOnDeathAsync();
        e.Sender.RefreshStat();

        PlayerRevivalPenalization playerRevivalPenalization = _revivalConfiguration.PlayerRevivalPenalization;
        if (e.Sender.PlayerEntity.Level <= playerRevivalPenalization.MaxLevelWithoutRevivalPenalization)
        {
            return;
        }

        int amount = e.Sender.PlayerEntity.Level < playerRevivalPenalization.MaxLevelWithDignityPenalizationIncrement
            ? e.Sender.PlayerEntity.Level * playerRevivalPenalization.DignityPenalizationIncrementMultiplier
            : playerRevivalPenalization.MaxLevelWithDignityPenalizationIncrement * playerRevivalPenalization.DignityPenalizationIncrementMultiplier;

        await e.Sender.PlayerEntity.RemoveDignity(amount, _minMaxConfiguration, _languageService, _reputationConfiguration, _rankingManager.TopReputation);
    }
}