// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;
using FamilyServer.Logs;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;

namespace FamilyServer.Consumers
{
    public class FamilyDeclareLogsMessageConsumer : IMessageConsumer<FamilyDeclareLogsMessage>
    {
        private readonly FamilyLogManager _familyLogManager;

        public FamilyDeclareLogsMessageConsumer(FamilyLogManager familyLogManager) => _familyLogManager = familyLogManager;

        public Task HandleAsync(FamilyDeclareLogsMessage e, CancellationToken cancellation)
        {
            _familyLogManager.SaveFamilyLogs(e.Logs);
            return Task.CompletedTask;
        }
    }
}