using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceMonsterDeathHandler : IAsyncEventProcessor<MonsterDeathEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceMonsterDeathHandler(IAsyncEventPipeline asyncEventPipeline, ITimeSpaceManager timeSpaceManager)
    {
        _asyncEventPipeline = asyncEventPipeline;
        _timeSpaceManager = timeSpaceManager;
    }

    public async Task HandleAsync(MonsterDeathEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monster = e.MonsterEntity;
        if (monster.MapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        if (e.IsByCommand)
        {
            return;
        }

        Guid guid = monster.MapInstance.Id;
        TimeSpaceParty timeSpace = _timeSpaceManager.GetTimeSpaceByMapInstanceId(guid);
        if (timeSpace == null)
        {
            return;
        }

        if (!timeSpace.Instance.TimeSpaceSubInstances.TryGetValue(guid, out TimeSpaceSubInstance timeSpaceSubInstance))
        {
            return;
        }

        timeSpace.Instance.IncreaseKilledMonsters();

        DateTime now = DateTime.UtcNow;
        if (timeSpaceSubInstance.LastTryFinishTime < now)
        {
            timeSpaceSubInstance.LastTryFinishTime = now.AddMilliseconds(300);
        }
        else
        {
            timeSpaceSubInstance.LastTryFinishTime += TimeSpan.FromMilliseconds(300);
        }

        timeSpace.LastObjectivesCheck = now + TimeSpan.FromSeconds(1);

        if (timeSpace.IsEasyMode)
        {
            return;
        }

        if (!monster.IsBonus)
        {
            await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceIncreaseScoreEvent
            {
                AmountToIncrease = 1,
                TimeSpaceParty = timeSpace
            });

            timeSpaceSubInstance.MonsterBonusCombo = 0;
            return;
        }

        timeSpaceSubInstance.MonsterBonusCombo++;

        int combo = 1 + 5 * timeSpaceSubInstance.MonsterBonusCombo;

        await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceIncreaseScoreEvent
        {
            AmountToIncrease = combo,
            TimeSpaceParty = timeSpace
        });

        timeSpaceSubInstance.MonsterBonusId = null;
        await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceBonusMonsterEvent
        {
            MonsterEntities = monster.MapInstance.GetAliveMonsters(),
            TimeSpaceSubInstance = timeSpaceSubInstance
        });
    }
}