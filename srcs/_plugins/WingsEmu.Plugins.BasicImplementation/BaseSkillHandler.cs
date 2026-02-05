using PhoenixLib.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game.Core.SkillHandling;
using WingsEmu.Game.Core.SkillHandling.Event;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations;

public class BaseSkillHandler : ISkillHandlerContainer
{
    private readonly Dictionary<long[], ISkillHandler> _handlerBySkillId = new();

    public void Register(ISkillHandler handler)
    {
        if (_handlerBySkillId.ContainsKey(handler.SkillId))
        {
            return;
        }

        Log.Debug($"[SKILL_HANDLER][REGISTER] SKILL_ID : {handler.SkillId} REGISTERED !");
        _handlerBySkillId.Add(handler.SkillId, handler);
    }

    public void Unregister(long[] skillId)
    {
        Log.Debug($"[SKILL_HANDLER][UNREGISTER] SKILL_ID : {skillId} UNREGISTERED !");
        _handlerBySkillId.Remove(skillId);
    }

    public void Handle(IClientSession player, SkillEvent args)
    {
        HandleAsync(player, args).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task HandleAsync(IClientSession player, SkillEvent args)
    {
        long skillId = args.SkillId;
        foreach (ISkillHandler handler in from key in _handlerBySkillId.Keys where key.Contains(skillId) select _handlerBySkillId[key])
        {
            Log.Debug($"[SKILL_HANDLER][HANDLING] : {args.SkillId} ");
            await handler.ExecuteAsync(player, args);
            break;
        }
    }
}