using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game._i18n;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("interchannel.broadcast.msg")]
    public class FamilyShoutMessage : IMessage
    {
        public string Message { get; set; }
        public string SenderName { get; set; }
        public long FamilyId { get; set; }
        public GameDialogKey GameDialogKey { get; set; }
    }
}