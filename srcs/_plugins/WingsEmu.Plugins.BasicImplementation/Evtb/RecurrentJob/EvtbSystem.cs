using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using WingsAPI.Packets.Enums;
using WingsEmu.Core.Extensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Evtb.RecurrentJob;

public class EvtbSystem : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private readonly IEvtbConfiguration _evtbConfiguration;
    private readonly GeneralEvtbFile _automaticEvtbConfiguration;
    private readonly ISessionManager _serverManager;
    private readonly IGameLanguageService _languageService;
    private readonly HashSet<(EvtbType, bool)> _notifiedEvents = [];

    public EvtbSystem(GeneralEvtbFile automaticEvtbConfiguration, IEvtbConfiguration evtbConfiguration, ISessionManager serverManager, IGameLanguageService languageService)
    {
        _automaticEvtbConfiguration = automaticEvtbConfiguration;
        _evtbConfiguration = evtbConfiguration;
        _serverManager = serverManager;
        _languageService = languageService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[EVTB_SYSTEM] Started!");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Process();
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task Process()
    {
        DateTime currentTime = DateTime.UtcNow;
        _evtbConfiguration.DesactivateExpiredEvents(currentTime, _automaticEvtbConfiguration.AutomaticEvents);

        foreach (AutomaticEvtbFile automaticEvtbFile in _automaticEvtbConfiguration.AutomaticEvents)
        {
            bool eventIsActive = currentTime.IsBetweenTwoDates(automaticEvtbFile.StartDateTime, automaticEvtbFile.EndDateTime);
            
            foreach (ConfigurationEvtb configurationEvtb in automaticEvtbFile.Events)
            {
                IClientSession session = _serverManager.Sessions.FirstOrDefault();
                
                if (session == null)
                {
                    continue;
                }

                switch (eventIsActive)
                {
                    case true when !_notifiedEvents.Contains((configurationEvtb.EventType, false)):
                        await NotifyEventStart(session, configurationEvtb.EventType, configurationEvtb);
                        break;
                    case false when _notifiedEvents.Contains((configurationEvtb.EventType, false)) && !_notifiedEvents.Contains((configurationEvtb.EventType, true)):
                        await NotifyEventEnd(session, configurationEvtb.EventType, configurationEvtb);
                        break;
                }
            }
        }
    }

    private async Task NotifyEventStart(IClientSession session, EvtbType eventType, ConfigurationEvtb configurationEvtb)
    {
        _evtbConfiguration.ActivateEvent(eventType, configurationEvtb.Value);
        string readableEventName = GetReadableEventName(eventType);
        string formattedValue = FormatEventValue(eventType, configurationEvtb.Value);

        await session.EmitEventAsync(new ChatShoutAdminEvent
        {
            Message = _languageService.GetLanguageFormat(GameDialogKey.EVTB_NOTIFY_EVENT_AUTOMATIC_START, session.UserLanguage, readableEventName, formattedValue)
        });
        session.SendEvtbPacket(_evtbConfiguration);
        _notifiedEvents.Add((eventType, false));
    }

    private async Task NotifyEventEnd(IClientSession session, EvtbType eventType, ConfigurationEvtb configurationEvtb)
    {
        string readableEventName = GetReadableEventName(eventType);
        string formattedValue = FormatEventValue(eventType, configurationEvtb.Value);

        await session.EmitEventAsync(new ChatShoutAdminEvent
        {
            Message = _languageService.GetLanguageFormat(GameDialogKey.EVTB_NOTIFY_EVENT_AUTOMATIC_ENDED, session.UserLanguage, readableEventName, formattedValue)
        });
        session.SendEvtbPacket(_evtbConfiguration);
        _notifiedEvents.Add((eventType, true));
    }

    private static string FormatEventValue(EvtbType eventType, double value)
    {
        switch (eventType)
        {
            case EvtbType.INCREASE_CHANCE_UPGRADE_EQUIPMENT:
            case EvtbType.INCREASE_CHANCE_GAMBLING_EQUIPMENT:
            case EvtbType.INCREASE_CHANCE_UPGRADE_SPECIALIST:
            case EvtbType.INCREASE_CHANCE_UPGRADE_SP_PERFECTION:
            case EvtbType.INCREASE_FAMILY_XP_EARNED:
            case EvtbType.INCREASE_CHANCE_UPGRADE_RUNES:
            case EvtbType.INCREASE_CHANCE_UPGRADE_TATTOOS:
            case EvtbType.INCREASE_FISHING_EXPERIENCE_GAIN:
            case EvtbType.INCREASE_COOKING_EXPERIENCE_GAIN:
            case EvtbType.INCREASE_CHANCE_GET_SECOND_RAIDBOX:
            case EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED:
            case EvtbType.INCREASE_CHANCE_GET_HIGHER_PARTNER_SKILLS:
            case EvtbType.INCREASE_PARTNER_CARD_FUSION:
            case EvtbType.INCREASE_PET_TRAINER_EXPERIENCE:
            case EvtbType.INCREASE_CHANCE_FAIRY_UPGRADE:
                return $"{value}%";
            case EvtbType.IS_SEALED_EVENT_ACTIVE:
                return "Available";
            default:
                return $"x{value}";
        }
    }
    
    private static string GetReadableEventName(EvtbType eventType)
    {
        IEnumerable<string> words = eventType.ToString().Split('_').Select(word => char.ToUpper(word[0]) + word[1..].ToLower());
        
        return string.Join(" ", words);
    }
}
