using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class SpecialistRefreshEventHandler : IAsyncEventProcessor<SpecialistRefreshEvent>
{
    private readonly GameMinMaxConfiguration _gameMinMaxConfiguration;
    private readonly IExpirableLockService _lockService;

    public SpecialistRefreshEventHandler(IExpirableLockService lockService, GameMinMaxConfiguration gameMinMaxConfiguration)
    {
        _lockService = lockService;
        _gameMinMaxConfiguration = gameMinMaxConfiguration;
    }

    public async Task HandleAsync(SpecialistRefreshEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        bool canRefresh = await _lockService.TryAddTemporaryLockAsync($"game:locks:specialist-points-refresh:{session.PlayerEntity.Id}", DateTime.UtcNow.Date.AddDays(1));

        if (canRefresh == false && e.Force == false)
        {
            session.SendDebugMessage("Specialist Points already refreshed today.");
            return;
        }

        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.SpecialistMedal) && session.PlayerEntity.SpPointsBonus + 30000 < _gameMinMaxConfiguration.MaxSpAdditionalPoints)
        {
            session.PlayerEntity.SpPointsBonus += 30000;
        }
        else
        {
            session.PlayerEntity.SpPointsBonus = _gameMinMaxConfiguration.MaxSpBasePoints;
        }


        session.PlayerEntity.SpPointsBasic = _gameMinMaxConfiguration.MaxSpBasePoints;
        session.RefreshSpPoint();
    }
}