using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsAPI.Packets.Enums.PartnerFusion;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Mates.PartnerFusion;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class SpptPacketHandler : GenericGamePacketHandlerBase<SpptPacket>
{
    private readonly PartnerFusionDataConfiguration _fusionData;
    private readonly IPartnerFusionAlgorithm _fusionAlgorithm;

    public SpptPacketHandler(PartnerFusionDataConfiguration fusionData, IPartnerFusionAlgorithm fusionAlgorithm)
    {
        _fusionData = fusionData;
        _fusionAlgorithm = fusionAlgorithm;
    }

    protected override async Task HandlePacketAsync(IClientSession session, SpptPacket packet)
    {
        PartnerFusionSlotType slotType = packet.SlotType;
        short slot = packet.Slot;
        short? slot2 = packet.Slot2;

        InventoryItem psp = session.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);

        if (psp == null)
        {
            return;
        }

        if (!psp.ItemInstance.GameItem.IsPartnerSpecialist)
        {
            return;
        }
        
        ItemVnums itemVnum = (ElementType)psp.ItemInstance.GameItem.Element switch
        {
            ElementType.Fire => ItemVnums.SMALL_RUBY_COMP,
            ElementType.Water => ItemVnums.SMALL_SAPPHIRE_COMP,
            ElementType.Shadow => ItemVnums.SMALL_OBSIDIAN_COMP,
            ElementType.Light => ItemVnums.SMALL_TOPAZ_COMP
        };
        
        PartnerFusionData fusionConfig = _fusionData.PartnerFusionData.FirstOrDefault(s =>
            s.LevelRange.Minimum <= psp.ItemInstance.SpStoneUpgrade && s.LevelRange.Maximum >= psp.ItemInstance.SpStoneUpgrade);

        if (fusionConfig == null)
        {
            return;
        }

        if (slotType == PartnerFusionSlotType.Upgrade)
        {
            session.SendPtspInsertPacket(psp.ItemInstance.SpStoneUpgrade, (int)psp.ItemInstance.Xp, -1, -1);
            
            if (fusionConfig.LevelRange.Maximum == psp.ItemInstance.SpStoneUpgrade)
            {
                session.SendPtspUpdatePacket(fusionConfig.MajorUpgradeCost, (int)itemVnum, fusionConfig.MajorItemAmount);
            }
            else
            {
                session.SendPtspUpdatePacket(fusionConfig.UpgradeCost, fusionConfig.ItemRequired.Vnum, fusionConfig.ItemRequired.Amount);
            }
            
            return;
        }

        if (!slot2.HasValue)
        {
            return;
        }
        
        InventoryItem material = session.PlayerEntity.GetItemBySlotAndType(slot2.Value, InventoryType.Equipment);

        if (material == null)
        {
            return;
        }

        if (!material.ItemInstance.GameItem.IsPartnerSpecialist)
        {
            return;
        }

        bool sameMorph = psp.ItemInstance.GameItem.Morph == material.ItemInstance.GameItem.Morph;

        if (psp.ItemInstance.SpStoneUpgrade == fusionConfig.LevelRange.Maximum && !sameMorph)
        {
            return;
        }

        PartnerFusionInfo fusionInfo = _fusionAlgorithm.GetPartnerFusionData(psp.ItemInstance.SpStoneUpgrade, psp.ItemInstance.Xp, material.ItemInstance.SpStoneUpgrade, sameMorph);
        
        session.SendPtspInsertPacket(psp.ItemInstance.SpStoneUpgrade, psp.ItemInstance.Xp, fusionInfo.Level, fusionInfo.Percentage);

        if (fusionInfo.Level > fusionConfig.LevelRange.Maximum)
        {
            session.SendPtspUpdatePacket(fusionConfig.MajorUpgradeCost, (int)itemVnum, fusionConfig.MajorItemAmount);
            return;
        }
        
        session.SendPtspUpdatePacket(fusionConfig.UpgradeCost, fusionConfig.ItemRequired.Vnum, fusionConfig.ItemRequired.Amount);
    }
}