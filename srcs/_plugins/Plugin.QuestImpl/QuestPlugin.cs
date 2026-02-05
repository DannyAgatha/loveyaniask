using System;
using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;
using WingsEmu.Game.Quests;

namespace Plugin.QuestImpl
{
    public class QuestPlugin : IGamePlugin
    {
        private readonly ICommandContainer _commands;
        private readonly IRunScriptHandlerContainer _runScriptHandlerContainer;
        private readonly IServiceProvider _serviceProvider;

        public QuestPlugin(ICommandContainer commands, IRunScriptHandlerContainer runScriptHandlerContainer, IServiceProvider serviceProvider)
        {
            _commands = commands;
            _runScriptHandlerContainer = runScriptHandlerContainer;
            _serviceProvider = serviceProvider;
        }

        public string Name => nameof(QuestPlugin);

        public void OnLoad()
        {
            _commands.AddModule<QuestModule>();

            foreach (Type handlerType in typeof(QuestPlugin).Assembly.GetTypesImplementingInterface<IRunScriptHandler>())
            {
                try
                {
                    object tmp = _serviceProvider.GetService(handlerType);
                    if (tmp is not IRunScriptHandler real)
                    {
                        continue;
                    }

                    Log.Debug($"[RUN_SCRIPT][ADD_HANDLER] {handlerType}");
                    _runScriptHandlerContainer.RegisterAsync(real);
                }
                catch (Exception e)
                {
                    Log.Error("[RUN_SCRIPT][FAIL_ADD]", e);
                }
            }
        }
    }
}