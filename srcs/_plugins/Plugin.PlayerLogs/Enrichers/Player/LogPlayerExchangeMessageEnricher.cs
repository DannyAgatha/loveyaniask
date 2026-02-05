using System.Collections.Generic;
using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Player;
using WingsAPI.Data.Exchanges;
using WingsEmu.Game.Exchange.Event;

namespace Plugin.PlayerLogs.Enrichers.Player
{
    public class LogPlayerExchangeMessageEnricher : ILogMessageEnricher<ExchangeCompletedEvent, LogPlayerExchangeMessage>
    {
        public void Enrich(LogPlayerExchangeMessage log, ExchangeCompletedEvent e)
        {
            var senderItems = new List<LogPlayerExchangeItemInfo>();
            var targetItems = new List<LogPlayerExchangeItemInfo>();

            for (byte i = 0; i < e.SenderItems.Count; i++)
            {
                senderItems.Add(new LogPlayerExchangeItemInfo
                {
                    Slot = i,
                    Amount = e.SenderItems[i].Item2,
                    ItemInstance = e.SenderItems[i].Item1
                });
            }

            for (byte i = 0; i < e.TargetItems.Count; i++)
            {
                targetItems.Add(new LogPlayerExchangeItemInfo
                {
                    Slot = i,
                    Amount = e.TargetItems[i].Item2,
                    ItemInstance = e.TargetItems[i].Item1
                });
            }

            log.Gold = e.SenderGold;
            log.Items = senderItems;
            log.BankGold = e.SenderBankGold;
            log.TargetGold = e.TargetGold;
            log.TargetBankGold = e.TargetBankGold;
            log.TargetCharacterId = e.Target.PlayerEntity.Id;
            log.TargetCharacterName = e.Target.PlayerEntity.Name;
            log.TargetItems = targetItems;
        }
    }
}