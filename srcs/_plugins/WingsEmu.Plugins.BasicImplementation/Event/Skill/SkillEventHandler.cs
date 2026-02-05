using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Core.SkillHandling;
using WingsEmu.Game.Core.SkillHandling.Event;

namespace NosEmu.Plugins.BasicImplementations.Event.Skill;

public class SkillEventHandler : IAsyncEventProcessor<SkillEvent>
{
    private readonly ISkillHandlerContainer _skillHandler;

    public SkillEventHandler(ISkillHandlerContainer skillHandler) => _skillHandler = skillHandler;

    public async Task HandleAsync(SkillEvent e, CancellationToken cancellation)
    {
        await _skillHandler.HandleAsync(e.Sender, e);
    }
}