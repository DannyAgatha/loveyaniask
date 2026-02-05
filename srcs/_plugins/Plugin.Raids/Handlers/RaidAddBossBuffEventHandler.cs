using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids.Handlers;

public class RaidAddBossBuffEventHandler : IAsyncEventProcessor<RaidAddBossBuffEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IBuffFactory _buffFactory;
    public RaidAddBossBuffEventHandler(IGameLanguageService languageService, IBuffFactory buffComponent)
    {
        _buffFactory = buffComponent;
        _languageService = languageService;
    }
    public async Task HandleAsync(RaidAddBossBuffEvent e, CancellationToken cancellation)
    {
        IMonsterEntity raidBoss = e.MapInstance.GetAliveMonsters().FirstOrDefault(x => x.IsBoss);
        if (raidBoss == null)
        {
            return;
        }

        e.MapInstance.Broadcast(x => x.GenerateMsgPacket(
            _languageService.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_TARGETS_COMPLETED, x.UserLanguage), MsgMessageType.Middle));
        Buff buffToAdd = _buffFactory.CreateBuff(e.BuffId, raidBoss);
        await raidBoss.AddBuffAsync(buffToAdd);
    }
}