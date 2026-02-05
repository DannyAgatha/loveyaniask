using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Extensions;

namespace WingsEmu.Health.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void AddMaintenanceMode(this IServiceCollection services)
        {
            services.AddSingleton<IMaintenanceManager, MaintenanceManager>();
            if (!EnvironmentExtensions.IsFeatureActivated("SERVICE_HEALTHCHECK_ACTIVATED"))
            {
                return;
            }

            services.AddHostedService<HealthCheckHostedService>();
            services.AddMessageSubscriber<ServiceMaintenanceActivateMessage, MaintenanceActivateMessageConsumer>();
            services.AddMessageSubscriber<ServiceMaintenanceDeactivateMessage, MaintenanceDeactivateMessageConsumer>();
            services.AddMessagePublisher<ServiceStatusUpdateMessage>();
        }
    }
}