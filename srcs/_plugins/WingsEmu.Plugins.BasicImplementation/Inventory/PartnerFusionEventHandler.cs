using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Mates.PartnerFusion;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class PartnerFusionEventHandler : IAsyncEventProcessor<PartnerFusionEvent>
{
    private readonly IPartnerFusionAlgorithm _fusionAlgorithm;
    private readonly PartnerFusionDataConfiguration _fusionData;
    private readonly IEvtbConfiguration _evtbConfiguration;

    public PartnerFusionEventHandler(IPartnerFusionAlgorithm fusionAlgorithm, PartnerFusionDataConfiguration fusionData, IEvtbConfiguration evtbConfiguration)
    {
        _fusionAlgorithm = fusionAlgorithm;
        _fusionData = fusionData;
        _evtbConfiguration = evtbConfiguration;
    }

    public async Task HandleAsync(PartnerFusionEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        InventoryItem psp = e.Psp;
        InventoryItem material = e.Material;
        
        if (!psp.ItemInstance.GameItem.IsPartnerSpecialist)
        {
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }

        if (!material.ItemInstance.GameItem.IsPartnerSpecialist)
        {
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }

        PartnerFusionData fusionConfig = _fusionData.PartnerFusionData.FirstOrDefault(s => psp.ItemInstance.SpStoneUpgrade >= s.LevelRange.Minimum && psp.ItemInstance.SpStoneUpgrade <= s.LevelRange.Maximum);

        if (fusionConfig == null)
        {
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }
        
        bool sameMorph = psp.ItemInstance.GameItem.Morph == material.ItemInstance.GameItem.Morph;
        
        if (psp.ItemInstance.SpStoneUpgrade == fusionConfig.LevelRange.Maximum && !sameMorph)
        {
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }
        
        PartnerFusionInfo fusionData = _fusionAlgorithm.GetPartnerFusionData(psp.ItemInstance.SpStoneUpgrade, psp.ItemInstance.Xp, material.ItemInstance.SpStoneUpgrade, sameMorph);

        if (fusionData == null)
        {
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }

        bool majorUp = fusionData.Level > fusionConfig.LevelRange.Maximum;
        
        ItemVnums itemVNum = (ElementType)psp.ItemInstance.GameItem.Element switch
        {
            ElementType.Fire => ItemVnums.SMALL_RUBY_COMP,
            ElementType.Water => ItemVnums.SMALL_SAPPHIRE_COMP,
            ElementType.Shadow => ItemVnums.SMALL_OBSIDIAN_COMP,
            ElementType.Light => ItemVnums.SMALL_TOPAZ_COMP
        };
        
        long price = majorUp ? fusionConfig.MajorUpgradeCost : fusionConfig.UpgradeCost;
        int vNum = majorUp ? (int)itemVNum : fusionConfig.ItemRequired.Vnum;
        int amount = majorUp ? fusionConfig.MajorItemAmount : fusionConfig.ItemRequired.Amount;
        int points = majorUp ? fusionConfig.MajorPoints : 0;
        int levelDifference = fusionData.Level - psp.ItemInstance.SpStoneUpgrade;
        
        if (majorUp)
        {
            PartnerFusionData newPspFusionRange = _fusionData.PartnerFusionData.FirstOrDefault(s
                => s.LevelRange.Minimum <= fusionData.Level && s.LevelRange.Maximum >= fusionData.Level);

            if (newPspFusionRange == null)
            {
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
                return;
            }

            if (psp.ItemInstance.SpStoneUpgrade < fusionConfig.LevelRange.Maximum)
            {
                int previousRange = fusionConfig.LevelRange.Maximum - psp.ItemInstance.SpStoneUpgrade;
                points += fusionConfig.Points * previousRange;
                levelDifference -= previousRange;
            }

            points -= newPspFusionRange.Points;
            points += newPspFusionRange.Points * levelDifference;
        }
        else
        {
            points += fusionConfig.Points * levelDifference;
        }
        
        if (session.PlayerEntity.CountItemWithVnum(vNum) < amount)
        {
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.SendMsg(session.GetLanguage(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS), MsgMessageType.Middle);
            return;
        }
        
        if (!session.HasEnoughGold(price))
        {
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.SendInfo(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD));
            return;
        }
        
        if (!session.PlayerEntity.RemoveItemFromSlotAndType(material.Slot, InventoryType.Equipment, out InventoryItem item))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS), MsgMessageType.Middle);
            return;
        }
        
        session.PlayerEntity.RemoveGold(price);
        await session.RemoveItemFromInventory(vNum, (short)amount);
        session.SendInventoryRemovePacket(InventoryType.Equipment, material.Slot);
        
        psp.ItemInstance.SpStoneUpgrade = fusionData.Level;
        psp.ItemInstance.Xp = (int)(fusionData.Percentage * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_PARTNER_CARD_FUSION) * 0.01));
        

        PartnerFusionResult result = new();
        
        for (int i = 0; i < points; i++)
        {
            SpPerfStats stat = _fusionAlgorithm.GetPartnerRandomStat(psp.ItemInstance);

            switch (stat)
            {
                case SpPerfStats.Attack:
                    psp.ItemInstance.SpDamage++;
                    result.Damage++;
                    break;
                
                case SpPerfStats.Defense:
                    psp.ItemInstance.SpDefence++;
                    result.Defence++;
                    break;
                
                case SpPerfStats.CriticalDefence:
                    psp.ItemInstance.SpCriticalDefense++;
                    result.CriticalDefence++;
                    break;
                
                case SpPerfStats.HpMp:
                    psp.ItemInstance.SpHP++;
                    result.HpMp++;
                    break;
                
                case SpPerfStats.ResistanceFire:
                    psp.ItemInstance.SpFire++;
                    result.FireRes++;
                    break;
                
                case SpPerfStats.ResistanceWater:
                    psp.ItemInstance.SpWater++;
                    result.WaterRes++;
                    break;
                
                case SpPerfStats.ResistanceLight:
                    psp.ItemInstance.SpLight++;
                    result.LightRes++;
                    break;
                
                case SpPerfStats.ResistanceDark:
                    psp.ItemInstance.SpDark++;
                    result.ShadowRes++;
                    break;
            }
        }

        string infoMessage = $"Upgrade Level: +{levelDifference}\n\n";
        infoMessage += $"The specialist partner card's skill points have been increased by {points}.\n";

        List<(Func<int, bool> Check, string Message)> statInfo = PartnerFusionExtensions.GetStatInfo();

        for (int i = 0; i < statInfo.Count; i++)
        {
            if (!statInfo[i].Check(result.GetStatValue(i)))
            {
                continue;
            }

            int increase = result.GetStatValue(i) * PartnerFusionExtensions.GetMultiplier(i);
            infoMessage += string.Format(statInfo[i].Message, increase, result.GetStatValue(i));
        }

        session.SendPacket(session.GenerateInfoPacket(infoMessage));

        session.BroadcastEffect(EffectType.UpgradeSuccess);
        session.SendInventoryAddPacket(psp);
        session.SendPtspUpgradePacket(psp.ItemInstance.SpStoneUpgrade, (int)psp.ItemInstance.Xp, psp.Slot);
    }
}