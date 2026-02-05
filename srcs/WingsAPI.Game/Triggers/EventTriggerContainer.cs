using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Core.Generics;

namespace WingsEmu.Game.Triggers;

public class EventTriggerContainer : IEventTriggerContainer
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly ConcurrentDictionary<string, ThreadSafeHashSet<RegisteredEvent>> _events = new();

    public EventTriggerContainer(IAsyncEventPipeline eventPipeline) => _eventPipeline = eventPipeline;

    public void AddEvent(string key, IAsyncEvent notification, bool removedOnTrigger = false)
    {
        if (!_events.TryGetValue(key, out ThreadSafeHashSet<RegisteredEvent> registeredEvents))
        {
            registeredEvents = new ThreadSafeHashSet<RegisteredEvent>();
            _events[key] = registeredEvents;
        }

        registeredEvents.Add(new RegisteredEvent(notification, removedOnTrigger));
    }

    public async Task TriggerEvents(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (!_events.TryGetValue(key, out ThreadSafeHashSet<RegisteredEvent> registeredEvents))
        {
            return;
        }

        if (registeredEvents == null || !registeredEvents.Any())
        {
            return;
        }

        try
        {
            var removeEvents = new ThreadSafeHashSet<RegisteredEvent>();
            foreach (RegisteredEvent registeredEvent in registeredEvents.Reverse().ToArray())
            {
                if (registeredEvent == null)
                {
                    continue;
                }

                await _eventPipeline.ProcessEventAsync(registeredEvent.Notification);
                if (registeredEvent.RemovedOnTrigger)
                {
                    removeEvents.Add(registeredEvent);
                }
            }

            foreach (RegisteredEvent registeredEvent in removeEvents)
            {
                registeredEvents.Remove(registeredEvent);
            }
        }
        catch (Exception e)
        {
            Log.Error("TriggerEvents", e);
        }
    }

    private class RegisteredEvent
    {
        public RegisteredEvent(IAsyncEvent notification, bool removedOnTrigger)
        {
            Notification = notification;
            RemovedOnTrigger = removedOnTrigger;
        }

        public IAsyncEvent Notification { get; }
        public bool RemovedOnTrigger { get; }
    }
}