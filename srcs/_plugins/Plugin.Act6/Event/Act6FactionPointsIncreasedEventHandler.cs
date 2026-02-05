using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Act6.Configuration;
using WingsEmu.Game.Act6.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace Plugin.Act6.Event
{
    public class Act6FactionPointsIncreaseEventHandler : IAsyncEventProcessor<Act6FactionPointsIncreaseEvent>
    {
        private readonly IAct6Manager _act6Manager;
        private readonly Act6Configuration _conf;
        public Act6FactionPointsIncreaseEventHandler(IAct6Manager act6Manager, Act6Configuration conf)
        {
            _act6Manager = act6Manager;
            _conf = conf;
        }

        public async Task HandleAsync(Act6FactionPointsIncreaseEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            if (_act6Manager.FactionPointsLocked)
            {
                return;
            }

            if (!session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_6_1))
            {
                return;
            }

            switch (session.CurrentMapInstance.MapId)
            {
                case >= 229 and <= 232:
                    _act6Manager.AddFactionPoints(FactionType.Angel, e.PointsToAdd * _conf.FactionPointsPerPveKill);
                    break;
                case >= 233 and <= 236:
                    _act6Manager.AddFactionPoints(FactionType.Demon, e.PointsToAdd * _conf.FactionPointsPerPveKill);
                    break;
            }
        }
    }
}