using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Pipelines.Sockets.Unofficial.Arenas;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids.Handlers;

public class RaidProcessLaurenaWavesEventHandler : IAsyncEventProcessor<RaidProcessBossMechanicsEvent>
{
    private readonly IRaidManager _raidManager;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly ISkillsManager _skillsManager;
    private readonly IBuffFactory _buffFactory;

    public RaidProcessLaurenaWavesEventHandler(IRaidManager raidManager, IAsyncEventPipeline eventPipeline, ISkillsManager skillsManager, IBuffFactory buffFactory)
    {
        _raidManager = raidManager;
        _eventPipeline = eventPipeline;
        _skillsManager = skillsManager;
        _buffFactory = buffFactory;
    }

    public async Task HandleAsync(RaidProcessBossMechanicsEvent e, CancellationToken cancellation)
    {
        IBattleEntity caster = e.BattleEntity;
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

        if (monsterEntity.MonsterVNum != (short)MonsterVnum.LAURENA)
        {
            return;
        }
        if (e.SkillInfo == null) return;
        switch (e.SkillInfo.Vnum )
        {
            case (int)SkillsVnums.LAURENA_BUFF_TELEPORT:
                monsterEntity.TeleportOnMap(53, 59);
                break;
            case (int)SkillsVnums.LAURENA_STORM:
                for (int i = 0; i < 50; i++)
                {
                    Position spawnPosition = monsterEntity.MapInstance.GetRandomPosition();
                    var toSummon = new ToSummon
                    {
                        VNum = (short)MonsterVnum.LAURENA_STORM,
                        IsHostile = true,
                        IsMoving = false,
                        SpawnCell = spawnPosition,
                        IgnoreSkillRange = true
                    };

                    await _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(monsterEntity.MapInstance, new List<ToSummon> { toSummon }));
                    await Task.Delay(100);
                }
                break;
            case (int)SkillsVnums.LAURENA_TRANSFORM_TARGET:

                SkillDTO skillInfo = _skillsManager.GetSkill((short)SkillsVnums.LAURENA_TRANSFORM_TARGET);
                IEnumerable<IBattleEntity> characters = monsterEntity.GetEnemiesInRange(monsterEntity, skillInfo.Range);
                int targetCount = characters.Count();
                targetCount = Math.Max(targetCount / 4, 1);
                var random = new Random();
                IEnumerable<IBattleEntity> selectedTargets = characters.OrderBy(_ => random.Next()).Take(targetCount).ToList();
                if (targetCount == 1)
                {
                    IBattleEntity targetEntity = selectedTargets.First();
                    if (targetEntity is IPlayerEntity player)
                    {
                        if (player.IsMorphed || player.Morph == (int)MorphType.PoisonousHamster || player.Morph == (int)MorphType.BrownBushi)
                            break;

                        bool isHamster = random.Next(2) == 0;
                        player.Morph = isHamster ? (int)MorphType.PoisonousHamster : (int)MorphType.BrownBushi;
                        short buffVNum = isHamster ? (short)BuffVnums.SCENT_OF_THE_CURSE_RAT : (short)BuffVnums.SCENT_OF_THE_CURSE_BUSHI;
                        Buff buff = _buffFactory.CreateBuff(buffVNum, player);
                        player.AddBuffAsync(buff);
                        player.MorphUpgrade = 1;
                        player.MorphUpgrade2 = 1;
                        player.Session?.BroadcastCMode();
                    }
                }
                else
                {
                    IEnumerable<IBattleEntity> hamsters = selectedTargets.Take(targetCount / 2);
                    IEnumerable<IBattleEntity> bushis = selectedTargets.Skip(targetCount / 2);

                    foreach (IBattleEntity morphedCharacter in hamsters)
                    {
                        if (morphedCharacter is IPlayerEntity player)
                        {
                            if (player.IsMorphed || player.Morph == (int)MorphType.PoisonousHamster || player.Morph == (int)MorphType.BrownBushi)
                                continue;

                            Buff buffRat = _buffFactory.CreateBuff((short)BuffVnums.SCENT_OF_THE_CURSE_RAT, player);
                            player.AddBuffAsync(buffRat);
                            player.Morph = (int)MorphType.PoisonousHamster;
                            player.MorphUpgrade = 0;
                            player.MorphUpgrade2 = 0;
                            player.Session?.BroadcastCMode();
                        }
                    }

                    foreach (IBattleEntity morphedCharacter in bushis)
                    {
                        if (morphedCharacter is IPlayerEntity player)
                        {
                            if (player.IsMorphed || player.Morph == (int)MorphType.PoisonousHamster || player.Morph == (int)MorphType.BrownBushi)
                                continue;

                            Buff buffBushi = _buffFactory.CreateBuff((short)BuffVnums.SCENT_OF_THE_CURSE_BUSHI, player);
                            player.AddBuffAsync(buffBushi);
                            player.Morph = (int)MorphType.BrownBushi;
                            player.MorphUpgrade = 0;
                            player.MorphUpgrade2 = 0;
                            player.Session?.BroadcastCMode();
                        }
                    }
                }
                break;
        }
    }
}
