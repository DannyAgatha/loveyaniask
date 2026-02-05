using Microsoft.Extensions.DependencyInjection;

namespace PhoenixLib.Scheduler.ReactiveX
{
    public static class SchedulerServiceCollectionExtensions
    {
        public static void AddScheduler(this IServiceCollection services)
        {
            services.AddTransient<IScheduler, ObservableScheduler>();
        }

        public static void AddCron(this IServiceCollection services)
        {
            services.AddTransient<ICron, ObservableCron>();
        }
    }
}