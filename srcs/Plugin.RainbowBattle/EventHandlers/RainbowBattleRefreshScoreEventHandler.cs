using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleRefreshScoreEventHandler : IAsyncEventProcessor<RainbowBattleRefreshScoreEvent>
    {
        public async Task HandleAsync(RainbowBattleRefreshScoreEvent e, CancellationToken cancellation)
        {
            RainbowBattleParty rainbowBattleParty = e.RainbowBattleParty;

            if (rainbowBattleParty == null)
            {
                return;
            }

            if (!rainbowBattleParty.Started)
            {
                return;
            }

            if (rainbowBattleParty.FinishTime != null)
            {
                return;
            }

            string redTeam = rainbowBattleParty.GenerateRainbowScore(RainbowBattleTeamType.Red);
            string blueTeam = rainbowBattleParty.GenerateRainbowScore(RainbowBattleTeamType.Blue);

            foreach (IClientSession red in rainbowBattleParty.RedTeam)
            {
                red.SendPacket(redTeam);
            }

            foreach (IClientSession blue in rainbowBattleParty.BlueTeam)
            {
                blue.SendPacket(blueTeam);
            }
        }
    }
}