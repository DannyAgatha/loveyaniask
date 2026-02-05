using System.Threading.Tasks;
using WingsEmu.Game.Exchange.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsAPI.Game.Extensions.ItemExtension.Inventory
{
    public static class ExchangeExtension
    {
        public static async Task CloseExchange(this IClientSession session, ExcCloseType type = ExcCloseType.Failed)
            => await session.EmitEventAsync(new ExchangeCloseEvent
            {
                Type = type
            });

        public static string GenerateEmptyExchangeWindow(this IClientSession session, long targetId) => $"exc_list 1 {targetId} -1";
        public static void SendEmptyExchangeWindow(this IClientSession session, long targetId) => session.SendPacket(session.GenerateEmptyExchangeWindow(targetId));

        public static string GenerateExchangeWindow(this IClientSession session, long targetId, int gold, long bankGold, string itemsPackets)
            => $"exc_list 1 {targetId} {gold} {bankGold / 1000} {(itemsPackets == string.Empty ? "-1" : itemsPackets)}";

        public static void SendExchangeWindow(this IClientSession session, long targetId, int gold, long bankGold, string itemsPackets)
            => session.SendPacket(session.GenerateExchangeWindow(targetId, gold, bankGold, itemsPackets));
    }
}