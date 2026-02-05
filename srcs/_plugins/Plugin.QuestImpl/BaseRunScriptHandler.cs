using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;

namespace Plugin.QuestImpl
{
    public class BaseRunScriptHandler : IRunScriptHandlerContainer
    {
        private readonly Dictionary<long, IRunScriptHandler> _handlersByRunId;

        public BaseRunScriptHandler() => _handlersByRunId = new Dictionary<long, IRunScriptHandler>();

        public async Task RegisterAsync(IRunScriptHandler handler)
        {
            foreach (int runId in handler.RunIds)
            {
                if (_handlersByRunId.ContainsKey(runId))
                {
                    Log.Debug($"[RUN_SCRIPT_HANDLER][REGISTER_WARNING] RUN_ID : {runId} IS ALREADY REGISTERED! IS IT DUPLICATED?");
                    continue;
                }

                Log.Debug($"[RUN_SCRIPT_HANDLER][REGISTER] RUN_ID : {runId} REGISTERED !");
                _handlersByRunId.Add(runId, handler);
            }
        }

        public async Task UnregisterAsync(long runId)
        {
            Log.Debug($"[RUN_SCRIPT_HANDLER][UNREGISTER] RUN_ID : {runId} UNREGISTERED !");
            _handlersByRunId.Remove(runId);
        }

        public void Handle(IClientSession player, RunScriptEvent args)
        {
            HandleAsync(player, args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task HandleAsync(IClientSession player, RunScriptEvent args)
        {
            if (!_handlersByRunId.TryGetValue(args.RunId, out IRunScriptHandler handler))
            {
                Log.Debug($"[RUN_SCRIPT_HANDLER] RUN_ID : {args.RunId} ");
                return;
            }

            Log.Debug($"[RUN_SCRIPT_HANDLER][HANDLING] : {args.RunId} ");
            await handler.ExecuteAsync(player, args);
        }
    }
}