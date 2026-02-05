// NosEmu
// 


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Families;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Shops;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops;
using WingsEmu.Game.Shops.Event;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl.Upgrades
{
    public class FamilyUpgradeBuyFromShopEventHandler : IAsyncEventProcessor<FamilyUpgradeBuyFromShopEvent>
    {
        private readonly FamilyConfiguration _familyConfiguration;
        private readonly IFamilyService _familyService;
        private readonly IItemsManager _itemsManager;

        public FamilyUpgradeBuyFromShopEventHandler(IFamilyService familyService, IItemsManager itemsManager, FamilyConfiguration familyConfiguration)
        {
            _familyService = familyService;
            _itemsManager = itemsManager;
            _familyConfiguration = familyConfiguration;
        }

        public async Task HandleAsync(FamilyUpgradeBuyFromShopEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;

            if (!session.PlayerEntity.IsHeadOfFamily())
            {
                return;
            }

            INpcEntity npcId = session.CurrentMapInstance.GetNpcById(e.NpcId);
            if (npcId == null)
            {
                return;
            }

            if (npcId.ShopNpc.MenuType != ShopNpcMenuType.FAMILIES)
            {
                return;
            }

            ShopItemDTO shopUpgrade = npcId.ShopNpc.ShopItems.FirstOrDefault(s => s.Slot == e.Slot);

            if (shopUpgrade == null)
            {
                return;
            }

            IGameItem item = _itemsManager.GetItem(shopUpgrade.ItemVNum);

            if (item == null)
            {
                return;
            }

            if (!Enum.TryParse(item.Data[2].ToString(), out FamilyUpgradeType familyUpgradeType))
            {
                return;
            }

            int minLevel = item.Data[0];
            int previousUpgrade = item.Data[1];

            IFamily family = session.PlayerEntity.Family;
            if (family.Level < minLevel)
            {
                return;
            }

            if (session.PlayerEntity.Gold < item.Price)
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD), MsgMessageType.Middle);
                return;
            }

            if (previousUpgrade != 0 && !family.HasAlreadyBoughtUpgrade(previousUpgrade))
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.FAMILY_MESSAGE_UPGRADE_NO_PREVIOUS), MsgMessageType.Middle);
                session.SendChatMessage(session.GetLanguage(GameDialogKey.FAMILY_MESSAGE_UPGRADE_NO_PREVIOUS), ChatMessageColorType.Red);
                return;
            }

            FamilyUpgradesConfiguration newUpgrade = _familyConfiguration.Upgrades.FirstOrDefault(x => x.UpgradeType == familyUpgradeType && x.UpgradeLevel == item.Data[3]);
            if (newUpgrade == null)
            {
                return;
            }

            FamilyUpgradeResponse response = await _familyService.TryAddFamilyUpgrade(new FamilyUpgradeRequest
            {
                FamilyId = family.Id,
                UpgradeId = shopUpgrade.ItemVNum,
                FamilyUpgradeType = familyUpgradeType,
                Value = newUpgrade.Value
            });

            switch (response.ResponseType)
            {
                case FamilyUpgradeAddResponseType.SUCCESS:
                    session.PlayerEntity.RemoveGold(item.Price);
                    session.SendInformationChatMessage(session.GetLanguage(GameDialogKey.FAMILY_CHATMESSAGE_UPGRADE_BOUGHT));
                    await session.EmitEventAsync(new ShopNpcListItemsEvent { NpcId = (int)e.NpcId, ShopType = shopUpgrade.Type });
                    await session.EmitEventAsync(new FamilyUpgradeBoughtEvent
                    {
                        FamilyId = family.Id,
                        UpgradeVnum = shopUpgrade.ItemVNum,
                        FamilyUpgradeType = familyUpgradeType,
                        UpgradeValue = newUpgrade.Value
                    });
                    await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.SpendXGoldToNpc, item.Price));
                    break;
                case FamilyUpgradeAddResponseType.MAINTENANCE_MODE:
                    session.SendErrorChatMessage(session.GetLanguage(GameDialogKey.FAMILY_INFO_SERVICE_MAINTENANCE_MODE));
                    break;
            }
        }
    }
}