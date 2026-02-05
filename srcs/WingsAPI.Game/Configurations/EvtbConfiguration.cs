using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Packets.Enums;
using WingsEmu.Core.Extensions;

namespace WingsEmu.Game.Configurations;

public interface IEvtbConfiguration
{
    int GetValueForEventType(EvtbType eventType);
    int ModifyValue(EvtbType eventType, int newValue);
    bool HasSameValue(EvtbType eventType, int value);
    void RemoveValue(EvtbType eventType);
    void ActivateEvent(EvtbType eventType, int newValue);
    void DesactivateExpiredEvents(DateTime currentTime, IEnumerable<AutomaticEvtbFile> automaticEvtbFiles);
}

public class EvtbConfiguration : IEvtbConfiguration
{
    private readonly EvtbFileConfiguration _evtbConfig;
    private readonly HashSet<EvtbType> _activeEvents = [];
        
    public EvtbConfiguration(EvtbFileConfiguration config) => _evtbConfig = config;
    
    public int GetValueForEventType(EvtbType eventType)
    {
        EvtbFile eventConfiguration = _evtbConfig.FirstOrDefault(e => e.EventType == eventType);
        return eventConfiguration?.Value ?? 0;
    }

    public int ModifyValue(EvtbType eventType, int newValue)
    {
        EvtbFile eventConfiguration = _evtbConfig.FirstOrDefault(e => e.EventType == eventType);
        if (eventConfiguration is null)
        {
            return 0;
        }
        return eventConfiguration.Value = newValue;
    }
    
    public bool HasSameValue(EvtbType eventType, int value)
    {
        EvtbFile eventConfiguration = _evtbConfig.FirstOrDefault(e => e.EventType == eventType);
        if (eventConfiguration is null)
        {
            return false;
        }

        return eventConfiguration.Value == value;
    }
    
    public void RemoveValue(EvtbType eventType)
    {
        EvtbFile eventConfiguration = _evtbConfig.FirstOrDefault(e => e.EventType == eventType);
        if (eventConfiguration != null)
        {
            _evtbConfig.Remove(eventConfiguration);
        }
    }
    
    public void ActivateEvent(EvtbType eventType, int newValue)
    {
        if (_activeEvents.Contains(eventType))
        {
            return;
        }

        ModifyValue(eventType, newValue);
        _activeEvents.Add(eventType);
    }

    public void DesactivateExpiredEvents(DateTime currentTime, IEnumerable<AutomaticEvtbFile> automaticEvtbFiles)
    {
        var allConfiguredEvents = automaticEvtbFiles
            .Where(e => currentTime.IsBetweenTwoDates(e.StartDateTime, e.EndDateTime))
            .SelectMany(e => e.Events)
            .Select(e => e.EventType)
            .ToHashSet();

        var eventsToRemove = _activeEvents.Where(e => !allConfiguredEvents.Contains(e)).ToList();

        foreach (EvtbType eventType in eventsToRemove)
        {
            RemoveValue(eventType);
            _activeEvents.Remove(eventType);
        }
    }
}

public class EvtbFileConfiguration : List<EvtbFile>
{
}

public class EvtbFile
{
    public EvtbType EventType { get; set; }
    public int Value { get; set; }
}