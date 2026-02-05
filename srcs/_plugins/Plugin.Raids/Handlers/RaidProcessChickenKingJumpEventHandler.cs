using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums;

namespace Plugin.Raids.Handlers;

public class RaidProcessChickenKingJumpEventHandler : IAsyncEventProcessor<RaidProcessBossMechanicsEvent>
{
    private readonly IRaidManager _raidManager;

    public RaidProcessChickenKingJumpEventHandler(IRaidManager raidManager)
    {
        _raidManager = raidManager;
    }
    
    
    private static readonly int[] JumpSkillVnums = { (int)SkillsVnums.CHICKEN_KING_JUMP, (int)SkillsVnums.CHICKEN_KING_JUMP_QUICK, (int)SkillsVnums.CHICKEN_KING_JUMP_FAST };

    public async Task HandleAsync(RaidProcessBossMechanicsEvent e, CancellationToken cancellation)
    {
        IBattleEntity lordDraco = e.BattleEntity;
        SkillInfo skill = e.SkillInfo;

        if (!JumpSkillVnums.Contains(skill.Vnum))
        {
            return;
        }

        if (lordDraco is not IMonsterEntity monsterEntity)
        {
            return;
        }

        if (!monsterEntity.IsAlive())
        {
            return;
        }

        if (!monsterEntity.IsJumping)
        {
            return;
        }

        monsterEntity.IsJumping = false;
        RaidParty raidParty = _raidManager.GetRaidPartyByMapInstanceId(monsterEntity.MapInstance.Id);
        if (raidParty?.Instance?.RaidSubInstances == null)
        {
            return;
        }

        if (raidParty.Finished)
        {
            return;
        }

        if (!raidParty.Instance.RaidSubInstances.TryGetValue(monsterEntity.MapInstance.Id, out RaidSubInstance raidSubInstance) || raidSubInstance?.MapInstance == null)
        {
            return;
        }

        if (raidSubInstance.SavedTargetPosition == null || raidSubInstance.SavedTargetPosition.Value == default)
        {
            return;
        }

        Position position = raidSubInstance.SavedTargetPosition.Value;
        monsterEntity.TeleportOnMap(position.X, position.Y);
    }
}