using System.Threading.Tasks;
using WingsEmu.Game.Core.SkillHandling.Event;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Core.SkillHandling;

public interface ISkillHandlerContainer
{
    void Register(ISkillHandler handler);

    void Unregister(long[] skillId);

    void Handle(IClientSession player, SkillEvent args);

    Task HandleAsync(IClientSession player, SkillEvent args);
}