using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Handlers;

public class RaidProcessJumpEventHandler : IAsyncEventProcessor<RaidProcessJumpEvent>
    {
        private readonly IRaidManager _raidManager;
        private readonly IRandomGenerator _randomGenerator;

        public RaidProcessJumpEventHandler(IRandomGenerator randomGenerator, IRaidManager raidManager)
        {
            _randomGenerator = randomGenerator;
            _raidManager = raidManager;
        }

        public async Task HandleAsync(RaidProcessJumpEvent e, CancellationToken cancellation)
        {
            IMonsterEntity monsterEntity = e.MonsterEntity;

            if (monsterEntity == null)
            {
                return;
            }

            if (!monsterEntity.IsAlive())
            {
                return;
            }

            if (monsterEntity.MonsterVNum != (short)MonsterVnum.LORD_DRACO)
            {
                return;
            }

            if (monsterEntity.MapInstance is not { MapInstanceType: MapInstanceType.RaidInstance })
            {
                return;
            }

            if (!monsterEntity.IsJumping)
            {
                return;
            }

            IReadOnlyCollection<IPlayerEntity> players = monsterEntity.MapInstance.GetAliveCharacters();
            if (players.Count < 1)
            {
                monsterEntity.IsJumping = false;
                return;
            }

            IPlayerEntity randomTarget = players.ElementAt(_randomGenerator.RandomNumber(players.Count));
            if (randomTarget == null)
            {
                monsterEntity.IsJumping = false;
                return;
            }

            RaidParty raidParty = _raidManager.GetRaidPartyByMapInstanceId(monsterEntity.MapInstance.Id);
            if (raidParty?.Instance?.RaidSubInstances == null)
            {
                monsterEntity.IsJumping = false;
                return;
            }

            if (raidParty.Finished)
            {
                monsterEntity.IsJumping = false;
                return;
            }

            if (!raidParty.Instance.RaidSubInstances.TryGetValue(monsterEntity.MapInstance.Id, out RaidSubInstance raidSubInstance) || raidSubInstance?.MapInstance == null)
            {
                monsterEntity.IsJumping = false;
                return;
            }

            raidSubInstance.SavedTargetPosition = randomTarget.Position;
            monsterEntity.BroadcastEffectGround(EffectType.LordDracoJump, randomTarget.Position.X, randomTarget.Position.Y, false);
        }
    }