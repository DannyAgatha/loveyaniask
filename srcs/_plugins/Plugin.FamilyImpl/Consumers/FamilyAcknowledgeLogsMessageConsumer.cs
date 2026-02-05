using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game.Families;
using WingsEmu.Game.Managers;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyAcknowledgeLogsMessageConsumer : IMessageConsumer<FamilyAcknowledgeLogsMessage>
    {
        private readonly IFamilyManager _familyManager;
        private readonly ISessionManager _sessionManager;

        public FamilyAcknowledgeLogsMessageConsumer(IFamilyManager familyManager, ISessionManager sessionManager)
        {
            _familyManager = familyManager;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(FamilyAcknowledgeLogsMessage e, CancellationToken cancellation)
        {
            _familyManager.AddToFamilyLogs(e.Logs);
            foreach (KeyValuePair<long, List<FamilyLogDto>> pair in e.Logs)
            {
                FamilyPacketExtensions.SendFamilyLogsToMembers(_familyManager.GetFamilyByFamilyId(pair.Key), _sessionManager);
            }
        }
    }
}