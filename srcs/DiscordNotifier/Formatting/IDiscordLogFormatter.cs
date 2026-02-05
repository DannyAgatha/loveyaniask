namespace DiscordNotifier.Formatting
{
    public interface IDiscordLogFormatter<TMessage>
    {
        LogType LogType { get; }
        bool TryFormat(TMessage message, out string formattedString);
    }
}