using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using WingsAPI.Communication.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyChangeFactionEventHandler : IAsyncEventProcessor<FamilyChangeFactionEvent>
    {
        private readonly IFamilyService _familyService;
        private readonly IGameLanguageService _gameLanguageService;
        private readonly IExpirableLockService _expirableLockService;

        public FamilyChangeFactionEventHandler(IGameLanguageService gameLanguageService, IFamilyService familyService, IExpirableLockService expirableLockService)
        {
            _gameLanguageService = gameLanguageService;
            _familyService = familyService;
            _expirableLockService = expirableLockService;
        }

        public async Task HandleAsync(FamilyChangeFactionEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            if (!session.PlayerEntity.IsInFamily())
            {
                return;
            }

            if (session.PlayerEntity.GetFamilyAuthority() != FamilyAuthority.Head)
            {
                session.SendInfo(_gameLanguageService.GetLanguage(GameDialogKey.FAMILY_INFO_NOT_FAMILY_HEAD, session.UserLanguage));
                return;
            }

            if (!Enum.TryParse(e.Faction.ToString(), out FactionType factionType))
            {
                return;
            }

            if (factionType == FactionType.Neutral)
            {
                return;
            }

            IFamily family = session.PlayerEntity.Family;
            
            string lockKey = $"game:locks:family:{family.Id}:change-faction";
            
            if (!await _expirableLockService.TryAddTemporaryLockAsync(lockKey, DateTime.UtcNow.AddDays(3)))
            {
                DateTime? lockExpiration = await _expirableLockService.GetLockExpirationAsync(lockKey);
                
                if (!lockExpiration.HasValue || lockExpiration.Value <= DateTime.UtcNow)
                {
                    return;
                }

                TimeSpan remainingTime = lockExpiration.Value - DateTime.UtcNow;
                string formattedTime = remainingTime.ToReadableString();
                session.SendInfo(_gameLanguageService.GetLanguageFormat(GameDialogKey.CANT_CHANGE_FACTION_FAMILY_DELAY, session.UserLanguage, formattedTime));
                return;
            }

            FamilyChangeFactionResponse response = await _familyService.ChangeFactionByIdAsync(new FamilyChangeFactionRequest
            {
                FamilyId = family.Id,
                NewFaction = factionType
            });

            switch (response.Status)
            {
                case FamilyChangeFactionResponseType.SUCCESS:
                    await session.RemoveItemFromInventory(factionType == FactionType.Angel ? (int)ItemVnums.ANGEL_EGG_FAMILY : (int)ItemVnums.DEMON_EGG_FAMILY);
                    break;
                case FamilyChangeFactionResponseType.ALREADY_THAT_FACTION:
                    session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.FAMILY_SHOUTMESSAGE_ALREADY_THAT_FACTION, session.UserLanguage), MsgMessageType.Middle);
                    break;
                case FamilyChangeFactionResponseType.UNDER_COOLDOWN:
                    session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.FAMILY_SHOUTMESSAGE_CHANGE_FACTION_UNDER_COOLDOWN, session.UserLanguage), MsgMessageType.Middle);
                    break;
            }
        }
    }
}