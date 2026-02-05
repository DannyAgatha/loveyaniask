using System.Threading.Tasks;
using PhoenixLib.Events;

namespace WingsEmu.Game;

public interface IEventTriggerContainer
{
    public void AddEvent(string key, IAsyncEvent notification, bool removedOnTrigger = false);
    public Task TriggerEvents(string key);
}