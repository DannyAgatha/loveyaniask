using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingsAPI.Data.Prestige;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsAPI.Packets.Enums.Prestige;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

internal class AnglerSkinHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;

    public AnglerSkinHandler(IGameLanguageService gameLanguage, IItemsManager itemsManager)
    {
        _gameLanguage = gameLanguage;
        _itemsManager = itemsManager;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => [306];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        CharacterPrestigeDto prestige = session.PlayerEntity.CharacterPrestigeDto;

        if (prestige?.Tasks is not { Count: > 0 })
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.PRESTIGE_NO_ACTIVE_TASKS));
            return;
        }

        IEnumerable<IGrouping<GameDialogKey, string>> taskGroups = prestige.Tasks
            .Select(task =>
            {
                string desc = task.GetDisplayTaskLabel(_gameLanguage, _itemsManager, session);
                string progress = $"{task.Progress:N0} / {task.RequiredAmount:N0}";
                string line = $"- {desc} – {progress}";
                GameDialogKey groupKey = task.TaskType switch
                {
                    PrestigeTaskType.COLLECT_GOLD or PrestigeTaskType.COLLECT_ITEM => GameDialogKey.PRESTIGE_TASK_GROUP_COLLECTION,
                    PrestigeTaskType.KILL_MONSTERS_BY_LEVEL or PrestigeTaskType.KILL_MONSTERS_BY_VNUM or PrestigeTaskType.KILL_MONSTER_BOSS_BY_VNUM => GameDialogKey.PRESTIGE_TASK_GROUP_COMBAT,
                    _ => GameDialogKey.PRESTIGE_TASK_GROUP_OTHER
                };
                return (groupKey, line, task.RequiredAmount);
            })
            .OrderByDescending(x => x.RequiredAmount)
            .GroupBy(x => x.groupKey, x => x.line);

        var sb = new StringBuilder();
        sb.AppendLine(session.GetLanguage(GameDialogKey.PRESTIGE_TASKS_STATUS));

        foreach (IGrouping<GameDialogKey, string> group in taskGroups)
        {
            sb.AppendLine()
                .AppendLine(session.GetLanguage(group.Key))
                .Append(string.Join(Environment.NewLine, group))
                .AppendLine();
        }

        session.SendInfo(sb.ToString());
    }
}