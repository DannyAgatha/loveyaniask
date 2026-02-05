using System;
using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.PacketHandling.Game.BattlePass
{
    public class OpenBattlePassPacketHandler : GenericGamePacketHandlerBase<BpOpenPacket>
    {
        private readonly BattlePassConfiguration _battlePassConfiguration;

        public OpenBattlePassPacketHandler(BattlePassConfiguration battlePassConfiguration)
        {
            _battlePassConfiguration = battlePassConfiguration;
        }

        protected override async Task HandlePacketAsync(IClientSession session, BpOpenPacket packet)
        {
            if (!_battlePassConfiguration.IsBattlePassEnabled) return;

            session.EmitEvent(new BattlePassQuestPacketEvent());
            session.EmitEvent(new BattlePassItemPacketEvent());
            session.SendPacket(new BptPacket()
            {
                MinutesUntilSeasonEnd = (long)Math.Round((_battlePassConfiguration.EndSeason - DateTime.Now).TotalMinutes)
            });
            session.SendPacket(new BpoPacket());
            session.PlayerEntity.HaveBpUiOpen = true;
        }
    }
}