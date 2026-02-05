using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Arena;

public class ArenaTalentsRegisterHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _langService;
    private readonly IServerManager _serverManager;

    public ArenaTalentsRegisterHandler(IGameLanguageService langService, IServerManager serverManager)
    {
        _langService = langService;
        _serverManager = serverManager;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ARENA_OF_TALENTS_REGISTRATION };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
    }
}