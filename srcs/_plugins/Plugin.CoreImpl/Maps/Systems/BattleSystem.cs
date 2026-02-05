using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._ECS;
using WingsEmu.Game._ECS.Systems;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.ServerPackets.Battle;

namespace Plugin.CoreImpl.Maps.Systems
{
    public class BattleSystem : IMapSystem, IBattleSystem
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly IBCardEffectHandlerContainer _bCardHandlerContainer;
        private readonly IBuffFactory _buffFactory;
        private readonly IGameLanguageService _gameLanguage;
        private readonly IMapInstance _mapInstance;
        private readonly IRandomGenerator _randomGenerator;
        private readonly ConcurrentQueue<BuffProcessable> _buffProcessables = new();
        private readonly ConcurrentQueue<BuffRequest> _buffRequests = new();
        private readonly ConcurrentQueue<HitProcessable> _hitProcessables = new();
        private readonly ConcurrentQueue<HitRequest> _hitRequests = new();

        public BattleSystem(IAsyncEventPipeline asyncEventPipeline, IMapInstance mapInstance, IBCardEffectHandlerContainer bCardHandlerContainer, IGameLanguageService gameLanguage,
            IBuffFactory buffFactory, IRandomGenerator randomGenerator)
        {
            _asyncEventPipeline = asyncEventPipeline;
            _mapInstance = mapInstance;
            _bCardHandlerContainer = bCardHandlerContainer;
            _gameLanguage = gameLanguage;
            _buffFactory = buffFactory;
            _randomGenerator = randomGenerator;
        }

        public void AddCastHitRequest(HitProcessable hitProcessable)
        {
            _hitProcessables.Enqueue(hitProcessable);
        }

        public void AddCastBuffRequest(BuffProcessable buffProcessable)
        {
            _buffProcessables.Enqueue(buffProcessable);
        }

        public void AddHitRequest(HitRequest hitRequest)
        {
            _hitRequests.Enqueue(hitRequest);
        }

        public void AddBuffRequest(BuffRequest buffRequest)
        {
            _buffRequests.Enqueue(buffRequest);
        }

        public void ProcessTick(DateTime date, bool isTickRefresh = false)
        {
            ProcessAttackSkillCast(date);
            ProcessBuffSkillCast(date);
            ProcessAttack();
            ProcessBuff();
        }

        public string Name => nameof(BattleSystem);

        public void PutIdleState()
        {
            Clear();
        }

        public void Clear()
        {
            _hitProcessables.Clear();
            _hitRequests.Clear();
            _buffProcessables.Clear();
            _buffRequests.Clear();
        }

        private bool IsInterrupted(in IBattleEntity entity)
        {
            if (entity is not IPlayerEntity character)
            {
                return false;
            }

            if (!character.SkillComponent.IsSkillInterrupted)
            {
                return false;
            }

            int focus = character.HitRate;
            focus += character.BCardComponent.GetAllBCardsInformation(BCardType.Target, (byte)AdditionalTypes.Target.MagicalConcentrationIncreased, character.Level).firstData;
            focus -= character.BCardComponent.GetAllBCardsInformation(BCardType.Target, (byte)AdditionalTypes.Target.MagicalConcentrationDecreased, character.Level).firstData;
            
            int bCard = 1;
            
            if (character.BCardComponent.HasBCard(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.ConcentrationIncrease))
            {
                bCard *= character.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.ConcentrationIncrease, character.Level).firstData;
                focus *= bCard;
            }
            
            if (character.BCardComponent.HasBCard(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.ConcentrationDecrease))
            {
                bCard *= character.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.ConcentrationDecrease, character.Level).firstData;
                bCard *= focus;
                focus -= bCard;
            }
            
            double chance = 100 / Math.PI * Math.Atan(0.015 * (focus - 130) + 2) + 58;
            if (_randomGenerator.RandomNumber() < chance)
            {
                return false;
            }

            character.SkillComponent.IsSkillInterrupted = false;
            character.SkillComponent.CanBeInterrupted = false;
            character.CancelCastingSkill();
            character.Session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.SKILL_CHATMESSAGE_WAS_INTERRUPTED, character.Session.UserLanguage), ChatMessageColorType.Red);
            return true;
        }

        private void ProcessAttackSkillCast(in DateTime date)
        {
            var requestsInPending = new List<HitProcessable>();
            while (_hitProcessables.TryDequeue(out HitProcessable hitProcessable))
            {
                DateTime time = hitProcessable.SkillCast.SkillEndCastTime;
                IBattleEntity caster = hitProcessable.Caster;

                if (time > date)
                {
                    requestsInPending.Add(hitProcessable);
                    continue;
                }
                
                if (IsJump(caster, hitProcessable))
                {
                    switch (hitProcessable.SkillCast.Skill.Vnum)
                    {
                        case (short)SkillsVnums.DRAGON_JUMP:
                        case (short)SkillsVnums.CARNO_JUMP:
                            hitProcessable.SkillCast.SkillEndCastTime = hitProcessable.SkillCast.SkillEndCastTime.AddSeconds(2);
                            break;
                    }

                    requestsInPending.Add(hitProcessable);
                    continue;
                }

                if (IsInterrupted(caster))
                {
                    continue;
                }

                IBattleEntity target = hitProcessable.Target;
                SkillInfo skillInfo = hitProcessable.SkillCast.Skill;
                Position position = hitProcessable.Position;
                
                ProcessRaidActions(caster, skillInfo);

                _asyncEventPipeline.ProcessEventAsync(new ProcessHitEvent(caster, target, skillInfo, position));
            }

            foreach (HitProcessable request in requestsInPending)
            {
                AddCastHitRequest(request);
            }
        }
        
        private void ProcessRaidActions(IBattleEntity caster, SkillInfo skill)
        {
            if (!caster.IsMonster())
            {
                return;
            }

            MapInstanceType? instanceType = caster.MapInstance?.MapInstanceType;

            if (instanceType is not MapInstanceType.RaidInstance and not MapInstanceType.LandOfLife)
            {
                return;
            }

            _asyncEventPipeline.ProcessEventAsync(new RaidProcessBossMechanicsEvent
            {
                BattleEntity = caster,
                SkillInfo = skill
            });
        }

        private static readonly int[] JumpSkillVnums = { (int)SkillsVnums.DRAGON_JUMP, (int)SkillsVnums.CHICKEN_KING_JUMP, (int)SkillsVnums.CHICKEN_KING_JUMP_QUICK, (int)SkillsVnums.CHICKEN_KING_JUMP_FAST, (int)SkillsVnums.ASGOBAS_JUMP };
        
        private bool IsJump(IBattleEntity caster, HitProcessable hitProcessable)
        {
            if (hitProcessable.IsJump)
            {
                return false;
            }

            if (!caster.IsMonster())
            {
                return false;
            }

            if (caster is not IMonsterEntity monsterEntity)
            {
                return false;
            }

            if (!JumpSkillVnums.Contains(hitProcessable.SkillCast.Skill.Vnum))
            {
                return false;
            }

            hitProcessable.IsJump = true;
            _asyncEventPipeline.ProcessEventAsync(new RaidProcessJumpEvent
            {
                MonsterEntity = monsterEntity
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            return true;
        }

        private void ProcessBuffSkillCast(in DateTime date)
        {
            var requestsInPending = new List<BuffProcessable>();
            while (_buffProcessables.TryDequeue(out BuffProcessable buffProcessable))
            {
                DateTime time = buffProcessable.SkillCast.SkillEndCastTime;
                IBattleEntity caster = buffProcessable.Caster;

                if (time > date)
                {
                    requestsInPending.Add(buffProcessable);
                    continue;
                }

                if (IsInterrupted(caster))
                {
                    continue;
                }

                IBattleEntity target = buffProcessable.Target;
                SkillCast skillCast = buffProcessable.SkillCast;
                Position position = buffProcessable.Position;
                
                ProcessRaidActions(caster, buffProcessable.SkillCast.Skill);

                _asyncEventPipeline.ProcessEventAsync(new ProcessBuffEvent(caster, target, skillCast, position));
            }

            foreach (BuffProcessable request in requestsInPending)
            {
                AddCastBuffRequest(request);
            }
        }

        private void ProcessBuff()
        {
            while (_buffRequests.TryDequeue(out BuffRequest request))
            {
                SkillCast skillCast = request.SkillCast;
                SkillInfo skill = skillCast.Skill;
                IBattleEntity caster = request.Caster;

                caster.SetSkillCooldown(skill);
                request.Caster.RemoveCastingSkill();

                if (!caster.IsAlive())
                {
                    CancelHitRequest(caster, skill, request.Position, false, true);
                    return;
                }

                switch (skill.AttackType)
                {
                    case AttackType.Dash:
                        caster.ChangePosition(request.Position);
                        break;
                    case AttackType.Charge:
                    {
                        caster.BroadcastSuPacket(caster, skill, 0, SuPacketHitMode.NoDamageFail);
                        if (caster is not IPlayerEntity character)
                        {
                            continue;
                        }

                        character.BCardComponent.ClearChargeBCard();
                        bool addCharge = false;
                        foreach (BCardDTO bCard in skill.BCards.Where(x => x.Type == (short)BCardType.AttackPower))
                        {
                            addCharge = true;
                            character.BCardComponent.AddChargeBCard(bCard);
                        }

                        if (!addCharge && !character.HasBuff(BuffVnums.CHARGE))
                        {
                            continue;
                        }

                        Buff charge = _buffFactory.CreateBuff((short)BuffVnums.CHARGE, character);
                        character.AddBuffAsync(charge).ConfigureAwait(false).GetAwaiter().GetResult();
                        continue;
                    }
                }

                switch (skill.TargetAffectedEntities)
                {
                    case TargetAffectedEntities.DebuffForEnemies:
                        DebuffEnemies(request);
                        break;
                    case TargetAffectedEntities.BuffForAllies:
                        BuffAllies(request);
                        break;
                }

                if (caster is not IMonsterEntity monster)
                {
                    continue;
                }

                if (!monster.DisappearAfterHitting)
                {
                    continue;
                }

                _asyncEventPipeline.ProcessEventAsync(new MonsterDeathEvent(monster)).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private void BuffAllies(BuffRequest request)
        {
            SkillInfo skill = request.SkillCast.Skill;
            IBattleEntity caster = request.Caster;
            IEnumerable<IBattleEntity> entities = request.Targets;
            Position position = request.Position;

            BCardDTO[] afterAttackAllAllies = skill.BCardsType.TryGetValue(SkillCastType.AFTER_ATTACK_ALL_ALLIES, out HashSet<BCardDTO> allAllies) ? allAllies.ToArray() : Array.Empty<BCardDTO>();
            BCardDTO[] afterAttackAllTargets = skill.BCardsType.TryGetValue(SkillCastType.AFTER_ATTACK_ALL_TARGETS, out HashSet<BCardDTO> allTargets) ? allTargets.ToArray() : Array.Empty<BCardDTO>();

            foreach (BCardDTO bCard in afterAttackAllAllies)
            {
                _bCardHandlerContainer.Execute(caster, caster, bCard, skill, position);
            }

            if (skill.TargetType == TargetType.NonTarget)
            {
                caster.BroadcastSuPacket(caster, skill, 0, SuPacketHitMode.NoDamageSuccess);
                switch (skill.HitType)
                {
                    case TargetHitType.AlliesInAffectedAoE:
                        foreach (IBattleEntity entity in entities)
                        {
                            if (entity.IsInvisibleGm())
                            {
                                continue;
                            }

                            if (entity.Id != caster.Id)
                            {
                                caster.BroadcastSuPacket(entity, skill, 0, SuPacketHitMode.AttackedInAoe);
                                foreach (BCardDTO bCard in afterAttackAllAllies)
                                {
                                    _bCardHandlerContainer.Execute(entity, caster, bCard, skill, position);
                                }
                            }

                            foreach (BCardDTO bCard in afterAttackAllTargets)
                            {
                                _bCardHandlerContainer.Execute(entity, caster, bCard, skill, position);
                            }
                        }

                        break;
                    case TargetHitType.TargetOnly:
                        foreach (BCardDTO bCard in afterAttackAllTargets)
                        {
                            _bCardHandlerContainer.Execute(caster, caster, bCard, skill, position);
                        }

                        break;
                }
            }
            else
            {
                switch (skill.HitType)
                {
                    case TargetHitType.AlliesInAffectedAoE:
                        caster.BroadcastSuPacket(caster, skill, 0, SuPacketHitMode.NoDamageSuccess);
                        foreach (IBattleEntity entity in entities)
                        {
                            if (entity.IsInvisibleGm())
                            {
                                continue;
                            }

                            if (entity.Id != caster.Id)
                            {
                                caster.BroadcastSuPacket(entity, skill, 0, SuPacketHitMode.AttackedInAoe);
                                foreach (BCardDTO bCard in afterAttackAllAllies)
                                {
                                    _bCardHandlerContainer.Execute(entity, caster, bCard, skill, position);
                                }
                            }

                            foreach (BCardDTO bCard in afterAttackAllTargets)
                            {
                                _bCardHandlerContainer.Execute(entity, caster, bCard, skill, position);
                            }
                        }

                        break;
                    case TargetHitType.PlayerAndHisMates:
                        if (caster.IsInvisibleGm())
                        {
                            return;
                        }

                        switch (caster)
                        {
                            case IPlayerEntity character:
                            {
                                caster.BroadcastSuPacket(caster, skill, 0, SuPacketHitMode.NoDamageSuccess);
                                foreach (IBattleEntity entityMate in entities)
                                {
                                    caster.BroadcastSuPacket(entityMate, skill, 0, SuPacketHitMode.AttackedInAoe);
                                    foreach (BCardDTO bCard in afterAttackAllAllies)
                                    {
                                        _bCardHandlerContainer.Execute(entityMate, caster, bCard, skill, position);
                                    }

                                    foreach (BCardDTO bCard in afterAttackAllTargets)
                                    {
                                        _bCardHandlerContainer.Execute(entityMate, caster, bCard, skill, position);
                                    }
                                }

                                caster.BroadcastSuPacket(character, skill, 0, SuPacketHitMode.AttackedInAoe);
                                foreach (BCardDTO bCard in afterAttackAllTargets)
                                {
                                    _bCardHandlerContainer.Execute(character, caster, bCard, skill, position);
                                }

                                break;
                            }
                            case IMateEntity mate:
                            {
                                IPlayerEntity owner = mate.Owner;
                                IBattleEntity secondMate = owner.MateComponent.GetMate(m => mate.Position.IsInAoeZone(m.Position, skill.AoERange) && m.MateType != mate.MateType && m.IsTeamMember);

                                mate.BroadcastSuPacket(mate, skill, 0, SuPacketHitMode.AttackedInAoe);
                                if (secondMate != null)
                                {
                                    foreach (BCardDTO bCard in afterAttackAllAllies)
                                    {
                                        _bCardHandlerContainer.Execute(secondMate, caster, bCard, skill, position);
                                    }

                                    foreach (BCardDTO bCard in afterAttackAllTargets)
                                    {
                                        _bCardHandlerContainer.Execute(secondMate, caster, bCard, skill, position);
                                    }
                                }

                                foreach (BCardDTO bCard in afterAttackAllAllies)
                                {
                                    _bCardHandlerContainer.Execute(owner, caster, bCard, skill, position);
                                }

                                foreach (BCardDTO bCard in afterAttackAllTargets)
                                {
                                    _bCardHandlerContainer.Execute(owner, caster, bCard, skill, position);
                                }

                                foreach (BCardDTO bCard in afterAttackAllTargets)
                                {
                                    _bCardHandlerContainer.Execute(mate, caster, bCard, skill, position);
                                }

                                break;
                            }
                        }

                        break;
                    case TargetHitType.TargetOnly:
                        IBattleEntity target = entities.FirstOrDefault();
                        if (target == null)
                        {
                            return;
                        }

                        if (!target.IsAlive())
                        {
                            CancelHitRequest(caster, skill, request.Position);
                            return;
                        }

                        if (target.IsInvisibleGm())
                        {
                            return;
                        }

                        caster.BroadcastSuPacket(target, skill, 0, SuPacketHitMode.NoDamageSuccess);

                        if (caster.Id != target.Id)
                        {
                            foreach (BCardDTO bCard in afterAttackAllAllies)
                            {
                                _bCardHandlerContainer.Execute(target, caster, bCard, skill, position);
                            }
                        }

                        foreach (BCardDTO bCard in afterAttackAllTargets)
                        {
                            _bCardHandlerContainer.Execute(target, caster, bCard, skill, position);
                        }

                        break;
                }
                
                if (caster is not IPlayerEntity player)
                {
                    return;
                }

                switch (skill.Vnum)
                {
                    case (int)SkillsVnums.REEL_IN:
                        player?.FishingSecondFish?.Dispose();
                        player.Session.EmitEvent(new CollectFishEvent());
                        break;
                    case (int)SkillsVnums.CAST_LINE:
                    case (int)SkillsVnums.CAST_LINE_PRO:
                        player.Session.EmitEvent(new CastFishingLineEvent());
                        break;
                }
            }
        }

        private void DebuffEnemies(BuffRequest request)
        {
            SkillInfo skill = request.SkillCast.Skill;
            Position position = request.Position;
            IEnumerable<IBattleEntity> entities = request.Targets.ToList();
            IBattleEntity target = request.Target;
            IBattleEntity caster = request.Caster;

            BCardDTO[] afterAttackAllAllies = skill.BCardsType.TryGetValue(SkillCastType.AFTER_ATTACK_ALL_ALLIES, out HashSet<BCardDTO> allAllies) ? allAllies.ToArray() : Array.Empty<BCardDTO>();
            BCardDTO[] afterAttackAllTargets = skill.BCardsType.TryGetValue(SkillCastType.AFTER_ATTACK_ALL_TARGETS, out HashSet<BCardDTO> allTargets) ? allTargets.ToArray() : Array.Empty<BCardDTO>();

            if (target == null)
            {
                if (skill.TargetType != TargetType.NonTarget && skill.HitType != TargetHitType.TargetOnly)
                {
                    CancelHitRequest(caster, skill, request.Position, false, true);
                    return;
                }

                target = caster;
            }

            if (!target.IsAlive())
            {
                CancelHitRequest(caster, skill, request.Position);
                return;
            }

            if (!skill.BCards.Any(x => x.Type == (short)BCardType.Capture && x.SubType == (byte)AdditionalTypes.Capture.CaptureAnimal) &&
                !skill.BCards.Any(x => x.Type == (short)BCardType.LordHatus && x.SubType == (byte)AdditionalTypes.LordHatus.CommandSunWolf))
            {
                caster.BroadcastSuPacket(target, skill, 0, SuPacketHitMode.NoDamageFail);
            }

            if (skill.Vnum == (short)SkillsVnums.TAUNT)
            {
                caster.MapInstance.Broadcast(caster.GenerateEffectPacket(EffectType.Taunt));
                caster.MapInstance.Broadcast(target.GenerateEffectPacket(EffectType.Taunt));
                (caster as IPlayerEntity)?.Session.SendSound(SoundType.TAUNT_SKILL);
                (target as IPlayerEntity)?.Session.SendSound(SoundType.TAUNT_SKILL);
            }

            foreach (BCardDTO bCard in afterAttackAllAllies)
            {
                _bCardHandlerContainer.Execute(caster, caster, bCard, skill, position);
            }

            if (skill.TargetType == TargetType.NonTarget)
            {
                switch (skill.HitType)
                {
                    case TargetHitType.EnemiesInAffectedAoE:
                        foreach (IBattleEntity entity in entities)
                        {
                            if (entity.IsInvisibleGm())
                            {
                                continue;
                            }

                            caster.BroadcastSuPacket(entity, skill, 0, SuPacketHitMode.AttackedInAoe);
                            foreach (BCardDTO bCard in afterAttackAllAllies)
                            {
                                _bCardHandlerContainer.Execute(entity, caster, bCard, skill, position);
                            }

                            foreach (BCardDTO bCard in afterAttackAllTargets)
                            {
                                _bCardHandlerContainer.Execute(entity, caster, bCard, skill, position);
                            }
                        }

                        break;
                    case TargetHitType.TargetOnly:
                        if (target.IsInvisibleGm())
                        {
                            return;
                        }

                        caster.BroadcastSuPacket(target, skill, 0, SuPacketHitMode.NoDamageSuccess);

                        foreach (BCardDTO bCard in afterAttackAllTargets)
                        {
                            _bCardHandlerContainer.Execute(target, caster, bCard, skill, position);
                        }

                        break;
                }
            }
            else
            {
                switch (skill.HitType)
                {
                    case TargetHitType.EnemiesInAffectedAoE:
                        foreach (IBattleEntity entity in entities)
                        {
                            if (entity.IsInvisibleGm())
                            {
                                continue;
                            }

                            if (entity.Id != target.Id)
                            {
                                caster.BroadcastSuPacket(entity, skill, 0, SuPacketHitMode.AttackedInAoe);
                            }
                            else
                            {
                                caster.BroadcastSuPacket(target, skill, 0, SuPacketHitMode.NoDamageSuccess);
                            }

                            foreach (BCardDTO bCard in afterAttackAllTargets)
                            {
                                _bCardHandlerContainer.Execute(entity, caster, bCard, skill, position);
                            }
                        }

                        break;
                    case TargetHitType.TargetOnly:
                        if (target.IsInvisibleGm())
                        {
                            return;
                        }

                        if (!skill.BCards.Any(x => x.Type == (short)BCardType.Capture && x.SubType == (byte)AdditionalTypes.Capture.CaptureAnimal))
                        {
                            caster.BroadcastSuPacket(target, skill, 0, SuPacketHitMode.NoDamageSuccess);
                        }

                        foreach (BCardDTO bCard in afterAttackAllTargets)
                        {
                            _bCardHandlerContainer.Execute(target, caster, bCard, skill, position);
                        }

                        break;
                }
            }
        }

        private void ProcessAttack()
        {
            while (_hitRequests.TryDequeue(out HitRequest request))
            {
                IBattleEntity caster = request.EHitInformation.Caster;
                IBattleEntity mainTarget = request.MainTarget;
                SkillInfo skill = request.EHitInformation.Skill;
                if (caster == null)
                {
                    continue;
                }

                if (request?.EHitInformation == null)
                {
                    continue;
                }
                
                if (mainTarget != null && caster.MapInstance.MapId != mainTarget?.MapInstance.MapId)
                {
                    CancelHitRequest(caster, skill, request.EHitInformation.Position, true, true);
                    continue;
                }

                if (IsBombSkill(caster, skill, _bCardHandlerContainer))
                {
                    continue;
                }

                if (skill.AttackType == AttackType.Dash && request.EHitInformation.IsFirst)
                {
                    caster.ChangePosition(request.EHitInformation.Position);
                }

                if (!request.Targets.Any())
                {
                    CancelHitRequest(caster, skill, request.EHitInformation.Position, true, true);
                    continue;
                }

                caster.SetSkillCooldown(skill);
                caster.RemoveCastingSkill();

                bool cancelByMiss = false;

                int increaseDamageByCharge = 0;
                if (caster.ChargeComponent.GetCharge() != 0)
                {
                    increaseDamageByCharge += caster.ChargeComponent.GetCharge();
                    caster.ChargeComponent.ResetCharge();
                }

                foreach ((IBattleEntity target, DamageAlgorithmResult result) in request.Targets)
                {
                    if (request.EHitInformation.Caster.MapInstance?.Id != target.MapInstance?.Id)
                    {
                        continue;
                    }

                    if (cancelByMiss)
                    {
                        break;
                    }

                    if (target.IsSameEntity(mainTarget) && skill.TargetType != TargetType.NonTarget)
                    {
                        if (!mainTarget.IsAlive())
                        {
                            CancelHitRequest(caster, skill, request.EHitInformation.Position);
                            break;
                        }

                        if (result.HitType == HitType.Miss && skill.HitType == TargetHitType.SpecialArea && request.Targets.Count() > 1)
                        {
                            cancelByMiss = true;
                        }
                    }

                    result.Damages += increaseDamageByCharge;

                    _asyncEventPipeline.ProcessEventAsync(new ApplyHitEvent(target, result, request.EHitInformation)).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                if (caster is IPlayerEntity playerEntity && playerEntity.SkillComponent.OnyxMonster is not null)
                {
                    playerEntity.SkillComponent.OnyxMonster.BroadcastDie();
                    _asyncEventPipeline.ProcessEventAsync(new MonsterDeathEvent(playerEntity.SkillComponent.OnyxMonster)).ConfigureAwait(false).GetAwaiter().GetResult();
                    playerEntity.SkillComponent.OnyxMonster = null;
                }

                if (!caster.IsMonster())
                {
                    continue;
                }

                var monster = (IMonsterEntity)caster;
                if (!monster.DisappearAfterHitting)
                {
                    continue;
                }

                _asyncEventPipeline.ProcessEventAsync(new MonsterDeathEvent(monster)).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private bool IsBombSkill(IBattleEntity caster, SkillInfo skill, IBCardEffectHandlerContainer bCardEffectHandlerContainer)
        {
            if (caster is not IPlayerEntity character)
            {
                return false;
            }

            if (skill.Vnum != (short)SkillsVnums.BOMB)
            {
                return false;
            }

            if (character.SkillComponent.BombEntityId.HasValue)
            {
                return false;
            }

            character.BroadcastSuPacket(caster, skill, 0, SuPacketHitMode.NoDamageSuccess);
            character.SetSkillCooldown(skill);
            character.RemoveCastingSkill();
            foreach (BCardDTO x in skill.BCards)
            {
                bCardEffectHandlerContainer.Execute(character, character, x, skill);
            }

            return true;
        }

        private void CancelHitRequest(IBattleEntity caster, SkillInfo skill, Position position, bool setCooldown = false, bool isNoTargets = false)
        {
            if (skill.TargetType == TargetType.NonTarget)
            {
                caster.BroadcastNonTargetSkill(position, skill);
            }
            else
            {
                caster.BroadcastSuPacket(caster, skill, 0, isNoTargets ? SuPacketHitMode.NoDamageFail : SuPacketHitMode.OutOfRange, isFirst: true);
            }

            if (setCooldown)
            {
                caster.SetSkillCooldown(skill);
                caster.CancelCastingSkill();
            }

            switch (caster)
            {
                case IMonsterEntity monster:
                    if (monster.DisappearAfterHitting)
                    {
                        _asyncEventPipeline.ProcessEventAsync(new MonsterDeathEvent(monster)).ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    break;
            }
        }
    }
}