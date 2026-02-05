namespace PhoenixLib.Events
{
    public interface IEventProcessor<in T>
    where T : IEvent
    {
        void Handle(T e);
    }
}