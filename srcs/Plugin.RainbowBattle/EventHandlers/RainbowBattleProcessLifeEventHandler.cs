using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleProcessLifeEventHandler : IAsyncEventProcessor<RainbowBattleProcessLifeEvent>
    {
        public async Task HandleAsync(RainbowBattleProcessLifeEvent e, CancellationToken cancellation)
        {
            RainbowBattleParty rainbowBattleParty = e.RainbowBattleParty;

            string redTeam = rainbowBattleParty.GenerateRainbowBattleLive(RainbowBattleTeamType.Red);
            string blueTeam = rainbowBattleParty.GenerateRainbowBattleLive(RainbowBattleTeamType.Blue);

            foreach (IClientSession session in rainbowBattleParty.RedTeam)
            {
                session.SendPacket(redTeam);
            }

            foreach (IClientSession session in rainbowBattleParty.BlueTeam)
            {
                session.SendPacket(blueTeam);
            }
        }
    }
}