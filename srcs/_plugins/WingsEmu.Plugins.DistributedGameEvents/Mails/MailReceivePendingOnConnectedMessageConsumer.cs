using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.DTOs.Mails;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Language;

namespace WingsEmu.Plugins.DistributedGameEvents.Mails
{
    public class MailReceivePendingOnConnectedMessageConsumer : IMessageConsumer<MailReceivePendingOnConnectedMessage>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly IGameItemInstanceFactory _itemInstanceFactory;
        private readonly ISessionManager _sessionManager;

        public MailReceivePendingOnConnectedMessageConsumer(IGameLanguageService gameLanguage, ISessionManager sessionManager, IGameItemInstanceFactory itemInstanceFactory)
        {
            _gameLanguage = gameLanguage;
            _sessionManager = sessionManager;
            _itemInstanceFactory = itemInstanceFactory;
        }

        public async Task HandleAsync(MailReceivePendingOnConnectedMessage e, CancellationToken cancellation)
        {
            long characterId = e.CharacterId;
            List<CharacterMailDto> mails = e.Mails;
            IClientSession session = _sessionManager.GetSessionByCharacterId(characterId);
            if (session == null)
            {
                return;
            }

            foreach (CharacterMailDto mail in mails)
            {
                byte slot = session.GetNextMailSlot();
                GameItemInstance itemInstance = _itemInstanceFactory.CreateItem(mail.ItemInstance);
                var newMail = new CharacterMail(mail, slot, itemInstance);

                session.PlayerEntity.MailNoteComponent.AddMail(newMail);
                session.SendParcel(newMail);
            }
            
            session.SendSayi(ChatMessageColorType.Green, Game18NConstString.NewParcelArrived,4, mails.Count);
        }
    }
}