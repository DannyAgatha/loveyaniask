using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;

namespace Plugin.Raids.Handlers;

public class RaidProcessLordDracoHitEventHandler : IAsyncEventProcessor<BattleExecuteSkillEvent>
    {
        private readonly IBuffFactory _buffFactory;
        private readonly IRaidManager _raidManager;

        public RaidProcessLordDracoHitEventHandler(IBuffFactory buffFactory, IRaidManager raidManager)
        {
            _buffFactory = buffFactory;
            _raidManager = raidManager;
        }

        public async Task HandleAsync(BattleExecuteSkillEvent e, CancellationToken cancellation)
        {
            IBattleEntity caster = e.Entity;
            if (!caster.IsMonster())
            {
                return;
            }

            if (!caster.IsAlive())
            {
                return;
            }

            if (caster is not IMonsterEntity monsterEntity)
            {
                return;
            }

            if (monsterEntity.MonsterVNum != (short)MonsterVnum.LORD_DRACO)
            {
                return;
            }

            SkillInfo skill = e.SkillInfo;
            switch ((SkillsVnums)skill.Vnum)
            {
                case SkillsVnums.DRAGON_JUMP:

                    if (monsterEntity.IsJumping)
                    {
                        break;
                    }

                    foreach (IClientSession session in monsterEntity.MapInstance.Sessions)
                    {
                        session.SendSound(monsterEntity, SoundType.LORD_DRACO_JUMP);
                    }
                    
                    monsterEntity.MapInstance.Broadcast(monsterEntity.GenerateUntarget());
                    monsterEntity.IsJumping = true;

                    RaidParty raidParty = _raidManager.GetRaidPartyByMapInstanceId(caster.MapInstance.Id);
                    if (raidParty?.Instance?.RaidSubInstances == null)
                    {
                        return;
                    }

                    if (raidParty.Finished)
                    {
                        return;
                    }

                    if (!raidParty.Instance.RaidSubInstances.TryGetValue(caster.MapInstance.Id, out RaidSubInstance raidSubInstance) || raidSubInstance?.MapInstance == null)
                    {
                        return;
                    }

                    monsterEntity.HasSpawnRedCircle = true;
                    raidSubInstance.LordDracoMeteorsSpawn = DateTime.UtcNow.AddSeconds(2);

                    break;
                case SkillsVnums.DRAGON_STAGGER:

                    monsterEntity.HasSpawnRedCircle = false;
                    Buff badLandingShock = _buffFactory.CreateBuff((short)BuffVnums.BAD_LANDING_SHOCK, monsterEntity);
                    Buff perpetualShockwave = _buffFactory.CreateBuff((short)BuffVnums.PERPETUAL_SHOCKWAVE, monsterEntity);

                    await monsterEntity.AddBuffAsync(badLandingShock, perpetualShockwave);

                    foreach (IClientSession session in monsterEntity.MapInstance.Sessions)
                    {
                        session.SendSound(monsterEntity, SoundType.LORD_DRACO_BAD_LANDING);
                    }

                    break;
            }
        }
    }