namespace PhoenixLib.Events
{
    public interface IEventProcessorPipeline
    {
        void ProcessEvent(IEvent e);
        void ProcessEvent<T>(T e) where T : IEvent;
    }
}