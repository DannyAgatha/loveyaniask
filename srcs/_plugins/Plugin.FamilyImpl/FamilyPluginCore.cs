using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.FamilyImpl.Logs;
using Plugin.FamilyImpl.RecurrentJob;
using WingsAPI.Communication.Families;
using WingsAPI.Plugins;
using WingsAPI.Plugins.Extensions;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyPluginCore : IGameServerPlugin
    {
        public string Name => nameof(FamilyPluginCore);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            services.AddFamilyModule();
            services.AddEventHandlersInAssembly<FamilyPluginCore>();
            services.AddTypesImplementingInterfaceInAssembly<INpcDialogAsyncHandler>(typeof(FamilyPluginCore).Assembly);

            services.AddSingleton<IFamilyManager, FamilyManager>();
            services.AddSingleton<IFamilyExperienceManager, FamilyExperienceManager>();
            services.AddSingleton<IFamilyLogManager, FamilyLogManager>();
            
            services.AddHostedService<FamilyBuffSystem>();
            services.AddHostedService<FamilyLogSystem>();
            services.AddHostedService<FamilyExperienceSystem>();

            services.AddMessagePublisher<FamilyMissionsResetMessage>();
        }
    }
}