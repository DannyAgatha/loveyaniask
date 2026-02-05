using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Plugin.Act4.Extension;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Revival;

namespace Plugin.Act4.Event;

public class RevivalEventDungeonHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly IAct4DungeonManager _act4DungeonManager;
    private readonly Act4DungeonsConfiguration _act4DungeonsConfiguration;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public RevivalEventDungeonHandler(IAct4DungeonManager act4DungeonManager, Act4DungeonsConfiguration act4DungeonsConfiguration, IGameLanguageService gameLanguage,
        ISpPartnerConfiguration spPartnerConfiguration)
    {
        _act4DungeonManager = act4DungeonManager;
        _act4DungeonsConfiguration = act4DungeonsConfiguration;
        _gameLanguage = gameLanguage;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive())
        {
            return;
        }

        if (e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.Act4Dungeon)
        {
            return;
        }

        if (!e.Sender.PlayerEntity.IsInFamily())
        {
            return;
        }

        DungeonInstance dungeon = _act4DungeonManager.GetDungeon(e.Sender.PlayerEntity.Family.Id);
        if (dungeon == null)
        {
            return;
        }

        e.Sender.PlayerEntity.DisableRevival();

        if (dungeon.DungeonSubInstances.TryGetValue(e.Sender.CurrentMapInstance.Id, out DungeonSubInstance subInstance))
        {
            if (subInstance != null && subInstance.Bosses.Count > 0)
            {
                dungeon.PlayerDeathInBossRoom = true;
            }
        }

        if (e.RevivalType == RevivalType.DontPayRevival && e.Forced != ForcedType.HolyRevival)
        {
            e.Sender.UpdateVisibility();
            e.Sender.PlayerEntity.Hp = 1;
            e.Sender.PlayerEntity.Mp = 1;
            await e.Sender.PlayerEntity.Restore(restoreHealth: false, restoreMana: false, restoreMates: false);
            e.Sender.ChangeMap(dungeon.SpawnInstance.MapInstance, dungeon.SpawnPoint.X, dungeon.SpawnPoint.Y);
            e.Sender.SendBuffsPacket();
            return;
        }

        if (e.Forced != ForcedType.HolyRevival)
        {
            e.Sender.PlayerEntity.RemoveReputation((int)(e.Sender.GetDungeonReputationRequirement(_act4DungeonsConfiguration.DungeonEntryCostMultiplier) / 1.8));
        }

        e.Sender.UpdateVisibility();
        await e.Sender.PlayerEntity.Restore(restoreMates: false);
        e.Sender.BroadcastRevive();
        e.Sender.BroadcastInTeamMembers(_gameLanguage, _spPartnerConfiguration);
        e.Sender.RefreshParty(_spPartnerConfiguration);
        await e.Sender.CheckPartnerBuff();
        e.Sender.SendBuffsPacket();
    }
}