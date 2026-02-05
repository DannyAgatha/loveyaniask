using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Plugin.Raids.Const;
using WingsAPI.Packets.Enums;
using WingsEmu.Core.Extensions;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Enum;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids.RecurrentJob;

public class RaidSystem : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IRaidManager _raidManager;

    public RaidSystem(IRaidManager raidManager, IAsyncEventPipeline eventPipeline)
    {
        _raidManager = raidManager;
        _eventPipeline = eventPipeline;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[RAID_SYSTEM] Started!");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (RaidParty raid in _raidManager.Raids.ToArray())
                {
                    await ProcessRaid(raid);
                }
            }
            catch (Exception e)
            {
                Log.Error("[RAID_SYSTEM]", e);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessRaid(RaidParty raidParty)
    {
        DateTime currentDate = DateTime.UtcNow;
        await TryFinish(raidParty, currentDate);
        await TryRemove(raidParty, currentDate);
        await TryExecuteEventsAfterSlowMo(raidParty, currentDate);
        await TryRefreshInfo(raidParty);
        await TrySpawnMonsterByRaidWave(raidParty, currentDate);

        if (raidParty.Instance?.RaidSubInstances == null)
        {
            return;
        }

        foreach (RaidSubInstance raidSubInstance in raidParty.Instance.RaidSubInstances.Values)
        {
            await TrySpawnLordDracoMeteors(raidParty, raidSubInstance, currentDate);
        }
    }

    private async Task TrySpawnMonsterByRaidWave(RaidParty raidParty, DateTime currentDate)
    {
        if (!raidParty.Started || raidParty.Finished || raidParty.Instance == null)
        {
            return;
        }

        if (raidParty.Instance.RaidSubInstances == null)
        {
            return;
        }

        foreach (RaidSubInstance raidSubInstance in raidParty.Instance.RaidSubInstances.Values)
        {
            if (raidSubInstance == null)
            {
                continue;
            }

            if (!raidSubInstance.RaidWavesActivated)
            {
                continue;
            }

            if (!raidSubInstance.RaidWaves.Any())
            {
                continue;
            }

            if (raidSubInstance.LastRaidWave > currentDate)
            {
                continue;
            }

            RaidWave wave = raidSubInstance.RaidWaves.GetOrDefault(raidSubInstance.RaidWaveState);
            if (wave == null)
            {
                raidSubInstance.RaidWaveState = 0;
                raidSubInstance.LastRaidWave = currentDate;
                continue;
            }

            raidSubInstance.LastRaidWave = currentDate.AddSeconds(wave.TimeInSeconds);
            raidSubInstance.RaidWaveState++;

            await _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(raidSubInstance.MapInstance, wave.Monsters));

            if (wave.DisableShoutMessage)
            {
                continue;
            }

            raidSubInstance.MapInstance.Broadcast(x =>
                x.GenerateMsgPacket(x.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_MONSTERS_WAVE), MsgMessageType.Middle));
        }
    }

    private async Task TrySpawnLordDracoMeteors(RaidParty raidParty, RaidSubInstance raidSubInstance, DateTime currentDate)
    {
        if (raidParty.Type != RaidType.LordDraco)
        {
            return;
        }

        if (!raidParty.Started || raidParty.Finished)
        {
            return;
        }

        if (raidSubInstance?.MapInstance == null)
        {
            return;
        }

        if (raidSubInstance.LordDracoMeteorsSpawn is null)
        {
            return;
        }

        if (raidSubInstance.LordDracoMeteorsSpawn.Value > currentDate)
        {
            return;
        }

        List<ToSummon> toSummon = new();

        for (int i = 0; i < 20; i++)
        {
            toSummon.Add(new ToSummon
            {
                VNum = (short)MonsterVnum.LORD_DRACO_METEOR,
                IsHostile = true,
                IsMoving = true,
                IgnoreSkillRange = true,
                IncreaseTick = true
            });
        }

        raidSubInstance.LordDracoMeteorsSpawn = null;
        await _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(raidSubInstance.MapInstance, toSummon));
    }

    private async Task TryFinish(RaidParty raidParty, DateTime currentTime)
    {
        if (!raidParty.Started || raidParty.Finished || raidParty.Instance.FinishDate > currentTime)
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new RaidInstanceFinishEvent(raidParty, RaidFinishType.TimeIsUp));
    }

    private async Task TryRemove(RaidParty raidParty, DateTime currentTime)
    {
        if (!raidParty.Started || !raidParty.Finished || raidParty.Instance.RemoveDate > currentTime)
        {
            return;
        }

        Log.Warn($"REMOVING RAID MAP {raidParty.Instance.RemoveDate}");
        bool isMarathonMode = raidParty.Leader != null && raidParty.Leader.PlayerEntity.Raid.IsMarathonMode;
        await _eventPipeline.ProcessEventAsync(new RaidInstanceDestroyEvent(raidParty, isMarathonMode));
    }

    private async Task TryExecuteEventsAfterSlowMo(RaidParty raidParty, DateTime currentTime)
    {
        if (!raidParty.Started || !raidParty.Finished || raidParty.Instance.FinishSlowMoDate == null || raidParty.Instance.FinishSlowMoDate > currentTime)
        {
            return;
        }

        raidParty.Instance.SetFinishSlowMoDate(null);
        foreach (RaidSubInstance subInstance in raidParty.Instance.RaidSubInstances.Values)
        {
            await subInstance.TriggerEvents(RaidConstEventKeys.RaidSubInstanceAfterSlowMo);
        }
    }

    private async Task TryRefreshInfo(RaidParty raidParty)
    {
        if (!raidParty.Started || raidParty.Destroy)
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new RaidInstanceRefreshInfoEvent(raidParty));
    }
}