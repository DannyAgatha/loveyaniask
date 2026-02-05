namespace Plugin.PlayerLogs.Core
{
    public interface ILogMessageEnricher<in TEvent, in TLogMessage> where TLogMessage : IPlayerActionLogMessage
    {
        void Enrich(TLogMessage message, TEvent e);
    }
}