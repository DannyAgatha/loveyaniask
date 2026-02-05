using System.Threading.Tasks;
using WingsEmu.Game.Core.SkillHandling.Event;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Core.SkillHandling;

public interface ISkillHandler
{
    long[] SkillId { get; }

    Task ExecuteAsync(IClientSession session, SkillEvent e);
}