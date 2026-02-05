using System;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Services.Messages
{
    [MessageType("service.notify.maintenance")]
    public class ServiceMaintenanceNotificationMessage : IMessage
    {
        public ServiceMaintenanceNotificationType NotificationType { get; init; }
        public string Reason { get; init; }
        public TimeSpan TimeLeft { get; init; }
    }

    public enum ServiceMaintenanceNotificationType
    {
        Rescheduled,
        ScheduleStopped,
        ScheduleWarning,
        Executed,
        EmergencyExecuted,
        Lifted
    }
}