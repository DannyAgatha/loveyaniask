using System.Threading.Tasks;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class GenerateGuriGuriHandler : IGuriHandler
{
    private readonly IMeditationManager _meditation;

    public GenerateGuriGuriHandler(IMeditationManager meditation) => _meditation = meditation;

    public long GuriEffectId => 2;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        session.Broadcast(session.GenerateGuriPacket(2, 1), new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        session.PlayerEntity.RemoveMeditation(_meditation);
    }
}