// NosEmu
// 


using System;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Managers;

namespace Plugin.PlayerLogs.Core
{
    public class GenericPlayerEventLogMessageFactory<TEvent, TLogMessage> : IPlayerEventLogMessageFactory<TEvent, TLogMessage>
    where TEvent : PlayerEvent
    where TLogMessage : IPlayerActionLogMessage, new()
    {
        private readonly ILogMessageEnricher<TEvent, TLogMessage> _enricher;

        public GenericPlayerEventLogMessageFactory(ILogMessageEnricher<TEvent, TLogMessage> enricher) => _enricher = enricher;

        public TLogMessage CreateMessage(TEvent e)
        {
            var message = new TLogMessage
            {
                ChannelId = StaticServerManager.Instance.ChannelId,
                CharacterId = e.Sender.PlayerEntity?.Id ?? e.Sender.Account.Id,
                CharacterName = e.Sender.PlayerEntity?.Name ?? $"account-noauth-{e.Sender.Account.Name}",
                CreatedAt = DateTime.UtcNow,
                IpAddress = e.Sender.IpAddress
            };

            _enricher.Enrich(message, e);

            return message;
        }
    }
}