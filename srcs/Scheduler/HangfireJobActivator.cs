using System;
using Hangfire;

namespace WingsEmu.ClusterScheduler
{
    public class HangfireJobActivator : JobActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public HangfireJobActivator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public override object ActivateJob(Type type) => _serviceProvider.GetService(type);
    }
}