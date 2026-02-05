using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special
{
    public class PremiumBattlePassCoinHandler : IItemHandler
    {
        private readonly BattlePassConfiguration _conf;

        public PremiumBattlePassCoinHandler(BattlePassConfiguration conf)
        {
            _conf = conf;
        }
        
        public ItemType ItemType => ItemType.Special;
        public long[] Effects => new long[] { 328 };

        public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
        {
            if (!session.PlayerEntity.BattlePassOptionDto.HavePremium)
            {
                session.SendPacket($"say 1 {session.PlayerEntity.Id} 11 You can't use this item without the Premium Battle Pass !");
                return;
            }
            
            session.PlayerEntity.BattlePassOptionDto.Points += e.Item.ItemInstance.GameItem.EffectValue;
            await session.RemoveItemFromInventory(item: e.Item);
            session.SendPacket($"say 1 {session.PlayerEntity.Id} 11 You have received {e.Item.ItemInstance.GameItem.EffectValue} Battle Pass Points !");

            await session.EmitEventAsync(new BattlePassQuestPacketEvent());
            await session.EmitEventAsync(new BattlePassItemPacketEvent());
            session.SendPacket(new BptPacket()
            {
                MinutesUntilSeasonEnd = (long)Math.Round((_conf.EndSeason - DateTime.Now).TotalMinutes)
            });
        }
    }
}