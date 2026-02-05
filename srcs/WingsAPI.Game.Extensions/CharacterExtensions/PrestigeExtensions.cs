using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Data.Prestige;
using WingsAPI.Packets.Enums.Prestige;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations.Prestige;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.CharacterExtensions;

public static class PrestigeExtensions
{

    public static IEnumerable<PrestigeTaskDto> GetCompletedTasks(this CharacterPrestigeDto dto)
    {
        return dto.Tasks?.Where(t => t.Completed) ?? [];
    }

    public static IEnumerable<PrestigeTaskDto> GetPendingTasks(this CharacterPrestigeDto dto)
    {
        return dto.Tasks?.Where(t => !t.Completed) ?? [];
    }

    public static bool AllTasksCompleted(this CharacterPrestigeDto dto)
    {
        return dto.Tasks != null && dto.Tasks.All(t => t.Completed);
    }
    
    public static void EnsurePrestigeInitialized(this IPlayerEntity player, PrestigeConfiguration config, int level = 0)
    {
        player.CharacterPrestigeDto ??= new CharacterPrestigeDto();
        
        if (player.CharacterPrestigeDto.CurrentPrestigeLevel < 0)
        {
            player.CharacterPrestigeDto.CurrentPrestigeLevel = 0;
        }

        if (player.CharacterPrestigeDto.CurrentPrestigeLevel == 0 && level != 0)
        {
            player.CharacterPrestigeDto.CurrentPrestigeLevel = level;
        }

        player.CharacterPrestigeDto.Tasks ??= [];
        
        if (player.CharacterPrestigeDto.Tasks.Count == 0)
        {
            player.CharacterPrestigeDto.GenerateTasksForNextLevel(config);
        }
    }


    public static void GenerateTasksForNextLevel(this CharacterPrestigeDto dto, PrestigeConfiguration configuration)
    {
        dto.Tasks ??= [];
        dto.Tasks.Clear();

        // Intentar con el nivel actual
        PrestigeLevelConfig configLevel = configuration.PrestigeLevels
            .FirstOrDefault(pl => pl.Level == dto.CurrentPrestigeLevel);

        // Fallback: si no existe (p.ej. estás en 0 y YAML empieza en 1), saltar al primer nivel definido
        if (configLevel is null)
        {
            PrestigeLevelConfig first = configuration.PrestigeLevels.OrderBy(p => p.Level).FirstOrDefault();
            if (first is null)
            {
                return;
            }

            dto.CurrentPrestigeLevel = first.Level; // << importante
            configLevel = first;
        }

        foreach (PrestigeTaskConfig taskConfig in configLevel.PrestigeTasks ?? Enumerable.Empty<PrestigeTaskConfig>())
        {
            switch (taskConfig.QuestType)
            {
                case PrestigeTaskType.COLLECT_ITEM:
                    if (taskConfig.Items != null)
                    {
                        dto.Tasks.AddRange(taskConfig.Items.Select(i => CreateTask(taskConfig, itemVnum: i.ItemVnum, amount: i.Amount)));
                    }

                    break;

                case PrestigeTaskType.KILL_MONSTERS_BY_VNUM:
                case PrestigeTaskType.KILL_MONSTER_BOSS_BY_VNUM:
                    if (taskConfig.Monsters != null)
                    {
                        dto.Tasks.AddRange(taskConfig.Monsters.Select(m => CreateTask(taskConfig, monsterVnum: m.MonsterVnum, amount: m.Amount)));
                    }

                    break;

                default:
                    dto.Tasks.Add(CreateTask(taskConfig, amount: taskConfig.Amount));
                    break;
            }
        }

        static PrestigeTaskDto CreateTask(PrestigeTaskConfig cfg, int? itemVnum = null, int? monsterVnum = null, long amount = 0) =>
            new()
            {
                TaskType = cfg.QuestType,
                ItemVnum = itemVnum,
                MonsterVnum = monsterVnum,
                RequiredAmount = amount,
                MapVnum = cfg.MapVnum,
                RaidId = cfg.RaidId,
                LevelRangeMargin = cfg.LevelRangeMargin,
                Progress = 0
            };
    }
    
    public static string GetDisplayTaskLabel(
        this PrestigeTaskDto task,
        IGameLanguageService gameLanguage,
        IItemsManager itemManager,
        IClientSession session)
    {
        switch (task.TaskType)
        {
            case PrestigeTaskType.COLLECT_ITEM when task.ItemVnum is not null:
                return gameLanguage.GetLanguageFormat(
                    GameDialogKey.PRESTIGE_TASK_COLLECT_ITEM,
                    session.UserLanguage,
                    gameLanguage.GetItemName(itemManager.GetItem(task.ItemVnum.Value), session)
                );

            case PrestigeTaskType.KILL_MONSTERS_BY_VNUM or PrestigeTaskType.KILL_MONSTER_BOSS_BY_VNUM
                when task.MonsterVnum is not null:
                IMonsterData monsterData = StaticNpcMonsterManager.Instance.GetNpc(task.MonsterVnum.Value);
                return gameLanguage.GetLanguageFormat(
                    task.TaskType.GetTaskNameDialogKey(),
                    session.UserLanguage,
                    gameLanguage.GetNpcMonsterName(monsterData, session)
                );
            
            case PrestigeTaskType.COLLECT_GOLD:
            case PrestigeTaskType.SPEND_GOLD_NPC:
            case PrestigeTaskType.GAIN_FAME:
                return gameLanguage.GetLanguageFormat(
                    task.TaskType.GetTaskNameDialogKey(),
                    session.UserLanguage,
                    task.RequiredAmount.ToString("N0")
                );

            default:
                return gameLanguage.GetLanguage(task.TaskType.GetTaskNameDialogKey(), session.UserLanguage);
        }
    }
    
    public static GameDialogKey GetTaskNameDialogKey(this PrestigeTaskType taskType)
    {
        return taskType switch
        {
            PrestigeTaskType.KILL_MONSTERS_BY_LEVEL => GameDialogKey.PRESTIGE_TASK_KILL_MONSTERS_BY_LEVEL,
            PrestigeTaskType.KILL_MONSTERS_BY_VNUM => GameDialogKey.PRESTIGE_TASK_KILL_MONSTER_BY_VNUM,
            PrestigeTaskType.KILL_MONSTER_BOSS_BY_VNUM => GameDialogKey.PRESTIGE_TASK_KILL_MONSTER_BOSS_BY_VNUM,
            PrestigeTaskType.COMPLETE_RAID => GameDialogKey.PRESTIGE_TASK_COMPLETE_RAID,
            PrestigeTaskType.WIN_PVP_IN_MAP => GameDialogKey.PRESTIGE_TASK_WIN_PVP_IN_MAP,
            PrestigeTaskType.COLLECT_GOLD => GameDialogKey.PRESTIGE_TASK_COLLECT_GOLD,
            PrestigeTaskType.SPEND_GOLD_NPC => GameDialogKey.PRESTIGE_TASK_SPEND_GOLD_NPC,
            PrestigeTaskType.GAIN_FAME => GameDialogKey.PRESTIGE_TASK_GAIN_FAME,
            PrestigeTaskType.REACH_LEVEL => GameDialogKey.PRESTIGE_TASK_REACH_LEVEL,
            PrestigeTaskType.REACH_HERO_LEVEL => GameDialogKey.PRESTIGE_TASK_REACH_HERO_LEVEL,
            PrestigeTaskType.COLLECT_ITEM => GameDialogKey.PRESTIGE_TASK_COLLECT_ITEM,
            PrestigeTaskType.COMPLETE_ELEMENTAL_RAID => GameDialogKey.PRESTIGE_TASK_COMPLETE_ELEMENTAL_RAID,
            _ => throw new ArgumentOutOfRangeException(nameof(taskType), taskType, null)
        };
    }
}