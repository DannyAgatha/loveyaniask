using PhoenixLib.Events;

namespace WingsEmu.Game.PrivateMapInstances.Events;

public class DestroyPrivateMapInstanceEvent : IAsyncEvent
{
    public PrivateMapInstance PrivateMapInstance { get; set; }
}