using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Shops;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Families;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops;
using WingsEmu.Game.Shops.Event;

namespace NosEmu.Plugins.BasicImplementations.Shop;

public class ShopNpcListItemsEventHandler : IAsyncEventProcessor<ShopNpcListItemsEvent>
{
    private readonly IItemsManager _itemManager;
    private readonly ISkillsManager _skillManager;

    public ShopNpcListItemsEventHandler(IItemsManager itemManager, ISkillsManager skillManager)
    {
        _itemManager = itemManager;
        _skillManager = skillManager;
    }

    public async Task HandleAsync(ShopNpcListItemsEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        byte type = e.ShopType;
        int npcId = e.NpcId;

        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(npcId);
        if (npcEntity?.ShopNpc == null)
        {
            return;
        }

        HandleFamilyShopListing(session, npcEntity, type);
        HandleItemShopListing(session, npcEntity, type);
        HandleShopSkillListing(session, npcEntity, type);
    }

    private void HandleFamilyShopListing(IClientSession session, INpcEntity npcEntity, byte type)
    {
        if (npcEntity.ShopNpc.MenuType != ShopNpcMenuType.FAMILIES)
        {
            return;
        }

        if (npcEntity.ShopNpc.ShopItems.Count == 0)
        {
            return;
        }

        var shopList = new StringBuilder($"n_inv 2 {npcEntity.Id} 0");

        int i = 0;
        IFamily family = session.PlayerEntity.Family;
        foreach (ShopItemDTO shopItem in npcEntity.ShopNpc.ShopItems.Where(s => s.Type == type).OrderBy(x => x.ItemVNum))
        {
            IGameItem gameItemByVnum = _itemManager.GetItem(shopItem.ItemVNum);

            FamilyUpgradeBuyableState familyItemBuyableStat;

            if (family == null)
            {
                familyItemBuyableStat = FamilyUpgradeBuyableState.REQUIREMENTS_NOT_MET;
            }
            else
            {
                if (family.HasAlreadyBoughtUpgrade(gameItemByVnum.Id))
                {
                    familyItemBuyableStat = FamilyUpgradeBuyableState.ALREADY_OWNED;
                }
                else if (family.Level < gameItemByVnum.Effect)
                {
                    familyItemBuyableStat = FamilyUpgradeBuyableState.REQUIREMENTS_NOT_MET;
                }
                else
                {
                    familyItemBuyableStat = FamilyUpgradeBuyableState.AVAILABLE;
                }
            }

            shopList.AppendFormat(" {0}|{1}|{2}", shopItem.ItemVNum, (byte)familyItemBuyableStat, i);
            i++;
        }

        session.SendPacket(shopList.ToString());
    }

    private void HandleShopSkillListing(IClientSession session, INpcEntity npcEntity, byte type)
    {
        if (npcEntity.ShopNpc.MenuType != ShopNpcMenuType.SKILLS)
        {
            return;
        }

        int shopType = 0;
        var shopList = new StringBuilder();
        foreach (ShopSkillDTO skill in npcEntity.ShopNpc.ShopSkills.Where(s => s.Type.Equals(type)).OrderBy(s => s.Slot))
        {
            SkillDTO skillInfo = _skillManager.GetSkill(skill.SkillVNum);

            if (skill.Type != 0)
            {
                shopType = 1;
                if (skillInfo.Class == (byte)session.PlayerEntity.Class)
                {
                    shopList.Append($" {skillInfo.Id}");
                }
            }
            else
            {
                shopList.Append($" {skillInfo.Id}");
            }
        }

        session.SendPacket($"n_inv 2 {npcEntity.Id} 0 {shopType}{shopList}");
    }

    private void HandleItemShopListing(IClientSession session, INpcEntity npcEntity, byte type)
    {
        if (npcEntity.ShopNpc.MenuType != ShopNpcMenuType.ITEMS && npcEntity.ShopNpc.MenuType != ShopNpcMenuType.MINILAND)
        {
            return;
        }


        byte shopType = 100;
        double percent = 1;

        bool isOnAct4 = session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4);

        if (isOnAct4)
        {
            switch (session.PlayerEntity.GetDignityIco())
            {
                default:
                    percent = 1.5;
                    shopType = 150;
                    break;
                case 3:

                    percent = 1.6;
                    shopType = 160;
                    break;
                case 4:

                    percent = 1.7;
                    shopType = 170;
                    break;
                case 5:
                case 6:
                    percent = 2;
                    shopType = 200;
                    break;
            }
        }
        else
        {
            switch (session.PlayerEntity.GetDignityIco())
            {
                case 3:
                    percent = 1.1;
                    shopType = 110;
                    break;

                case 4:
                    percent = 1.2;
                    shopType = 120;
                    break;

                case 5:
                    percent = 1.5;
                    shopType = 150;
                    break;

                case 6:
                    percent = 1.5;
                    shopType = 150;
                    break;
            }
        }

        var shopList = new StringBuilder($"n_inv 2 {npcEntity.Id} 0 {shopType}");
        foreach (ShopItemDTO shopItem in npcEntity.ShopNpc.ShopItems.Where(s => s.Type == type))
        {
            IGameItem gameItemByVnum = _itemManager.GetItem(shopItem.ItemVNum);
            if (gameItemByVnum == null)
            {
                continue;
            }

            switch (gameItemByVnum.ReputPrice)
            {
                case > 0 when gameItemByVnum.Type == 0:
                    shopList.Append(
                        $" {(byte)gameItemByVnum.Type}.{shopItem.Slot}.{shopItem.ItemVNum}.{shopItem.Rare}.{(gameItemByVnum.IsColorable ? shopItem.Color : shopItem.Upgrade)}.{shopItem.Price ?? gameItemByVnum.ReputPrice}.0.0");
                    break;
                case > 0 when gameItemByVnum.Type != 0:
                    shopList.Append($" {(byte)gameItemByVnum.Type}.{shopItem.Slot}.{shopItem.ItemVNum}.-1.{shopItem.Price ?? gameItemByVnum.ReputPrice}");
                    break;
                default:
                {
                    shopList.Append(gameItemByVnum.Type != 0
                        ? $" {(byte)gameItemByVnum.Type}.{shopItem.Slot}.{shopItem.ItemVNum}.-1.{(shopItem.Price ?? gameItemByVnum.Price) * percent}"
                        : $" {(byte)gameItemByVnum.Type}.{shopItem.Slot}.{shopItem.ItemVNum}.{shopItem.Rare}.{(gameItemByVnum.IsColorable ? shopItem.Color : shopItem.Upgrade)}.{(shopItem.Price ?? gameItemByVnum.Price) * percent}.0.0");
                    break;
                }
            }
        }

        session.SendPacket(shopList.ToString());
    }
}