using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscordNotifier.Discord;
using PhoenixLib.Events;
using WingsAPI.Data.Prestige;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsAPI.Packets.Enums.Prestige;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Prestige;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Prestige;

public class PrestigeProgressEventHandler : IAsyncEventProcessor<PrestigeProgressEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;

    public PrestigeProgressEventHandler(
        IGameLanguageService gameLanguage,
        IItemsManager itemsManager)
    {
        _gameLanguage = gameLanguage;
        _itemsManager = itemsManager;
    }

    
    public async Task HandleAsync(PrestigeProgressEvent e, CancellationToken cancellation)
    {
        CharacterPrestigeDto prestige = e.Session.PlayerEntity.CharacterPrestigeDto;

        if (prestige?.Tasks == null)
        {
            return;
        }

        PrestigeTaskDto? task = e.TaskType switch
        {
            PrestigeTaskType.COLLECT_ITEM =>
                prestige.GetPendingTasks().FirstOrDefault(t =>
                    t.TaskType == e.TaskType &&
                    t.ItemVnum == e.ItemVnum),

            PrestigeTaskType.KILL_MONSTERS_BY_VNUM or PrestigeTaskType.KILL_MONSTER_BOSS_BY_VNUM =>
                prestige.GetPendingTasks().FirstOrDefault(t =>
                    t.TaskType == e.TaskType &&
                    t.MonsterVnum == e.MonsterVnum),

            _ =>
                prestige.GetPendingTasks().FirstOrDefault(t =>
                    t.TaskType == e.TaskType)
        };

        if (task is null)
        {
            return;
        }

        bool wasCompleted = task.Completed;

        task.Progress += e.Amount;
        
        if (task.Progress > task.RequiredAmount)
        {
            task.Progress = task.RequiredAmount;
        }

        string taskLabel = task.GetDisplayTaskLabel(_gameLanguage, _itemsManager, e.Session);

        e.Session.SendMsg(
            e.Session.GetLanguageFormat(GameDialogKey.PRESTIGE_PROGRESS_UPDATED, taskLabel, task.Progress, task.RequiredAmount),
            MsgMessageType.SmallMiddle);

        if (!wasCompleted && task.Completed)
        {
            e.Session.SendInfo(
                e.Session.GetLanguageFormat(GameDialogKey.PRESTIGE_TASK_COMPLETED, taskLabel));
        }

        if (prestige.AllTasksCompleted())
        {
            e.Session.SendInfo(e.Session.GetLanguage(GameDialogKey.PRESTIGE_TASKS_COMPLETED));
        }
    }
}