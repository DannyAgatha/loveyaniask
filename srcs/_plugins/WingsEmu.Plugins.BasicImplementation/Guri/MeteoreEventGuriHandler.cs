using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class MeteoreEventGuriHandler : IGuriHandler
{
    private readonly IServerManager _serverManager;

    public MeteoreEventGuriHandler(IServerManager serverManager) => _serverManager = serverManager;

    public long GuriEffectId => -1;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        /*if (!_serverManager.EventInWaiting && session.Character.IsWaitingForEvent)
        {
            Log.Debug($"The player is not registered for this event. GuriEffectId : {GuriEffectId}");
            return;
        }
        
        session.SendBsInfoPacket(BsInfoType.OpenWindow, GameType.DodgeMeteor, 50, QuequeWindow.WaitForEntry);
        session.SendEsfPacket(2);
        session.Character.IsWaitingForEvent = true;*/
    }
}