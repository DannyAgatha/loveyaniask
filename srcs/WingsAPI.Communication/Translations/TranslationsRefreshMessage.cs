using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Translations
{
    [MessageType("translations.refresh")]
    public class TranslationsRefreshMessage : IMessage
    {
        public bool IsFullReload { get; init; }
    }
}