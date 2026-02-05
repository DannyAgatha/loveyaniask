using System;
using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using Plugin.FamilyImpl.Commands;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;
using WingsEmu.Game._NpcDialog;

namespace Plugin.FamilyImpl
{
    public class FamilyPlugin : IGamePlugin
    {
        private readonly ICommandContainer _commands;
        private readonly IServiceProvider _container;
        private readonly INpcDialogHandlerContainer _handlers;

        public FamilyPlugin(ICommandContainer commands, INpcDialogHandlerContainer handlers, IServiceProvider container)
        {
            _commands = commands;
            _handlers = handlers;
            _container = container;
        }

        public string Name { get; } = nameof(FamilyPlugin);

        public void OnLoad()
        {
            _commands.AddModule<AdministratorFamilyModule>();
            _commands.AddModule<Achievements.AdministratorFamilyModule>();
            _commands.AddModule<Achievements.AdministratorFamilyModule.AchievementModule>();
            _commands.AddModule<Achievements.AdministratorFamilyModule.MissionsModule>();
            _commands.AddModule<FamilyModule>();
            _commands.AddModule<FamilyNostaleUiCommandsModule>();


            foreach (Type handlerType in typeof(FamilyPlugin).Assembly.GetTypesImplementingInterface<INpcDialogAsyncHandler>())
            {
                try
                {
                    object tmp = _container.GetService(handlerType);
                    if (tmp is not INpcDialogAsyncHandler real)
                    {
                        continue;
                    }

                    Log.Debug($"[NPC_DIALOG][ADD_HANDLER] {handlerType}");
                    _handlers.Register(real);
                }
                catch (Exception e)
                {
                    Log.Error("[NPC_DIALOG][FAIL_ADD]", e);
                }
            }
        }
    }
}