using System;
using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Player;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.Player
{
    public class LogPlayerDisconnectedMessageEnricher : ILogMessageEnricher<CharacterDisconnectedEvent, LogPlayerDisconnectedMessage>
    {
        public void Enrich(LogPlayerDisconnectedMessage message, CharacterDisconnectedEvent e)
        {
            message.HardwareId = e.Sender.HardwareId;
            message.SessionEnd = DateTime.UtcNow;
            message.SessionStart = e.Sender.PlayerEntity.GameStartDate;
            message.MasterAccountId = e.Sender.Account.MasterAccountId.ToString();
        }
    }
}