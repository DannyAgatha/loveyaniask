using System;
using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.BasicImplementations.Guri;

public class AlzanorGuriHandler : IGuriHandler
{
    private readonly IAlzanorManager _alzanorManager;

    public AlzanorGuriHandler(IAlzanorManager alzanorManager)
    {
        _alzanorManager = alzanorManager;
    }

    public long GuriEffectId => 509;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        Console.WriteLine("registering player");
        if (!_alzanorManager.IsRegistrationActive)
        {
            return;
        }
        _alzanorManager.RegisterPlayer(session.PlayerEntity.Id);
    }
}