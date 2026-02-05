using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Alzanor.RecurrentJob;

public class AlzanorSystem : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan Start = TimeSpan.FromMinutes(5);
    
    private static List<(TimeSpan, int, TimeType)> _times = new();
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IAlzanorManager _alzanorManager;
    private readonly ISessionManager _sessionManager;

    public AlzanorSystem(IAsyncEventPipeline eventPipeline, IAlzanorManager alzanorManager, ISessionManager sessionManager)
    {
        _eventPipeline = eventPipeline;
        _alzanorManager = alzanorManager;
        _sessionManager = sessionManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[ALZANOR_EVENT] Start Alzanor system...");

        _times = _alzanorManager.Warnings.ToList();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MainProcess();
            }
            catch (Exception e)
            {
                Log.Error("[ALZANOR_EVENT] ", e);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task MainProcess()
    {
        DateTime dateNow = DateTime.UtcNow;
        switch (_alzanorManager.IsRegistrationActive)
        {
            case true:
                await ProcessRegistration(dateNow);
                return;
            case false when !_alzanorManager.IsActive:
                ProcessTime(dateNow);
                await ProcessStart(dateNow);
                return;
        }
        if(_alzanorManager.IsActive && !_alzanorManager.AlzanorParties.Any())
        {
            _alzanorManager.IsActive = false;
            _times.Clear();
            _times = _alzanorManager.Warnings.ToList();
            return;
        }

        foreach (AlzanorParty alzanorParty in _alzanorManager.AlzanorParties)
        {
            await ProcessAlzanorEvent(alzanorParty, dateNow);
        }
    }

    private async Task ProcessRegistration(DateTime dateTime)
    {
        if (_alzanorManager.RegistrationStartTime > dateTime)
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new AlzanorStartProcessRegistrationEvent());
    }
    
    private void ProcessTime(DateTime dateNow)
    {
        if (_alzanorManager.AlzanorProcessTime is null)
        {
            return;
        }

        if (_times.Count < 1)
        {
            return;
        }
        (TimeSpan, int, TimeType) warning = _times.MinBy(x => x.Item1);
        if (dateNow < _alzanorManager.AlzanorProcessTime + warning.Item1)
        {
            return;
        }
        _times.Remove(warning);
        GameDialogKey gameDialogKey = warning.Item3 == TimeType.SECONDS ? GameDialogKey.ALZANOR_SHOUTMESSAGE_PREPARATION_SECONDS : GameDialogKey.ALZANOR_SHOUTMESSAGE_PREPARATION_MINUTES;

        _sessionManager.Broadcast(x =>
        {
            return x.GenerateMsgPacket(x.GetLanguageFormat(gameDialogKey, warning.Item2), MsgMessageType.MiddleAndBottomCard);
        });
    }

    private async Task ProcessStart(DateTime dateNow)
    {
        if (_alzanorManager.AlzanorProcessTime is null)
        {
            return;
        }

        if (dateNow < _alzanorManager.AlzanorProcessTime + Start)
        {
            return;
        }

        _times.Clear();
        _times = _alzanorManager.Warnings.ToList();
        await _eventPipeline.ProcessEventAsync(new AlzanorStartRegisterEvent());
        
    }

    private async Task ProcessAlzanorEvent(AlzanorParty alzanorParty, DateTime dateTime)
    {
        ProcessStartGame(alzanorParty, dateTime);
        
        await TryEndAlzanorEvent(alzanorParty, dateTime);
        await TryDestroyAlzanorEvent(alzanorParty, dateTime);
        await ProcessTeamPoints(alzanorParty, dateTime);
    }

    private void ProcessStartGame(AlzanorParty alzanorParty, in DateTime time)
    {
        if (alzanorParty.Started)
        {
            return;
        }

        if (alzanorParty.StartTime.AddSeconds(5) > time)
        {
            return;
        }

        alzanorParty.Started = true;
        alzanorParty.LastMembersLife = time.AddSeconds(10);
        foreach (IClientSession session in alzanorParty.MapInstance.Sessions)
        {
            session.SendCondPacket();
        }

        alzanorParty.MapInstance.Broadcast(x => x.GenerateMsgPacket(x.GetLanguage(GameDialogKey.ALZANOR_SHOUTMESSAGE_START), MsgMessageType.Middle));
    }

    private async Task TryEndAlzanorEvent(AlzanorParty alzanorParty, DateTime dateTime)
    {
        if (alzanorParty.Winner.HasValue)
        {
            return;
        }

        if (!alzanorParty.Started || alzanorParty.EndTime > dateTime || alzanorParty.FinishTime != null)
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new AlzanorEndEvent
        {
            AlzanorParty = alzanorParty
        });
    }

    private async Task TryDestroyAlzanorEvent(AlzanorParty alzanorParty, DateTime dateTime)
    {
        if (alzanorParty.MapInstance != null && alzanorParty.MapInstance.Sessions.Count < 1)
        {
            Log.Warn("[ALZANOR_EVENT] Destroying Alzanor event instance");
            await _eventPipeline.ProcessEventAsync(new AlzanorDestroyEvent
            {
                AlzanorParty = alzanorParty
            });
            return;
        }

        if (!alzanorParty.Started || alzanorParty.FinishTime == null)
        {
            return;
        }

        if (alzanorParty.FinishTime > dateTime)
        {
            return;
        }

        Log.Warn("[ALZANOR_EVENT] Destroying Alzanor event instance");
        await _eventPipeline.ProcessEventAsync(new AlzanorDestroyEvent
        {
            AlzanorParty = alzanorParty
        });
    }

    private async Task ProcessTeamPoints(AlzanorParty alzanorParty, DateTime dateTime)
    {
        if (!alzanorParty.Started || alzanorParty.FinishTime != null)
        {
            return;
        }

        if (alzanorParty.LastPointsTeamAdd > dateTime)
        {
            return;
        }
        Console.WriteLine("ProcessTeamPoints");

        await _eventPipeline.ProcessEventAsync(new AlzanorRefreshScoreEvent
        {
            AlzanorParty = alzanorParty
        });

        await ProcessMembersLife(alzanorParty, dateTime);
    }
    
    private async Task ProcessMembersLife(AlzanorParty alzanorParty, DateTime dateTime)
    {
        if (!alzanorParty.Started || alzanorParty.FinishTime != null)
        {
            return;
        }

        if (alzanorParty.LastMembersLife > dateTime)
        {
            return;
        }

        alzanorParty.LastMembersLife = dateTime.AddSeconds(30);
        await _eventPipeline.ProcessEventAsync(new AlzanorProcessLifeEvent
        {
            AlzanorParty = alzanorParty
        });
    }
}