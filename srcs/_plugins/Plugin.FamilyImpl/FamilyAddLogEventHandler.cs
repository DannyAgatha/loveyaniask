using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Families;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;

namespace Plugin.FamilyImpl
{
    public class FamilyAddLogEventHandler : IAsyncEventProcessor<FamilyAddLogEvent>
    {
        private readonly IFamilyManager _familyManager;

        public FamilyAddLogEventHandler(IFamilyManager familyManager) => _familyManager = familyManager;

        public async Task HandleAsync(FamilyAddLogEvent e, CancellationToken cancellation)
        {
            FamilyLogDto log = e.Log;
            log.Timestamp = DateTime.UtcNow;
            log.FamilyId = e.Sender.PlayerEntity.Family.Id;
            _familyManager.SendLogToFamilyServer(e.Log);
        }
    }
}