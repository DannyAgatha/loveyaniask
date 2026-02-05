using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Enum;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids.Handlers;

public class RaidProcessGlacerusFreezeEventHandler : IAsyncEventProcessor<RaidProcessBossMechanicsEvent>
    {
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly IBuffFactory _buffFactory;
        private readonly IRaidManager _raidManager;

        public RaidProcessGlacerusFreezeEventHandler(IRaidManager raidManager, IBuffFactory buffFactory, IAsyncEventPipeline asyncEventPipeline)
        {
            _raidManager = raidManager;
            _buffFactory = buffFactory;
            _asyncEventPipeline = asyncEventPipeline;
        }

        public async Task HandleAsync(RaidProcessBossMechanicsEvent e, CancellationToken cancellation)
        {
            IBattleEntity glacerus = e.BattleEntity;
            SkillInfo skill = e.SkillInfo;

            if (skill.Vnum != (short)SkillsVnums.GLACERUS_FREEZE)
            {
                return;
            }

            if (glacerus is not IMonsterEntity monsterEntity)
            {
                return;
            }

            if (monsterEntity.MonsterVNum != (short)MonsterVnum.GLACERUS)
            {
                return;
            }

            if (!monsterEntity.IsAlive())
            {
                return;
            }

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

            if (raidSubInstance.GlacerusSafePositions is null)
            {
                return;
            }

            List<string> packets = new(3);
            int id = 0;
            foreach ((int effect, Position position) in raidSubInstance.GlacerusSafePositions)
            {
                packets.Add(UiPacketExtension.GenerateEffectGround(id, (EffectType)effect, position.X, position.Y, true));
                id++;
            }

            string stormPacket = monsterEntity.GenerateEffectPacket(EffectType.IceStorm);
            raidSubInstance.MapInstance.Broadcast(_ => stormPacket);

            foreach (INpcEntity npc in raidSubInstance.MapInstance.GetAliveNpcs())
            {
                if (npc.IsFrozenByGlacerus())
                {
                    continue;
                }

                if (IsInSquare(raidSubInstance.GlacerusSafePositions, npc))
                {
                    continue;
                }

                Buff eternalIce = _buffFactory.CreateBuff((short)BuffVnums.ETERNAL_ICE, npc);
                await npc.AddBuffAsync(eternalIce);
            }

            foreach (IClientSession session in raidSubInstance.MapInstance.Sessions)
            {
                session.SendPackets(packets);

                if (!session.PlayerEntity.IsAlive())
                {
                    continue;
                }

                if (session.PlayerEntity.IsFrozenByGlacerus())
                {
                    continue;
                }

                if (IsInSquare(raidSubInstance.GlacerusSafePositions, session.PlayerEntity))
                {
                    continue;
                }

                session.SendMsg(session.GetLanguage("GLACERUS_FREEZE_PLAYER"), MsgMessageType.SmallMiddle);
                Buff eternalIce = _buffFactory.CreateBuff((short)BuffVnums.ETERNAL_ICE, session.PlayerEntity);
                await session.PlayerEntity.AddBuffAsync(eternalIce);

                foreach (IMateEntity mateEntity in session.PlayerEntity.MateComponent.TeamMembers())
                {
                    mateEntity.BroadcastMateOut();
                }
            }

            if (raidSubInstance.MapInstance.Sessions.All(x => x?.PlayerEntity != null && x.PlayerEntity.IsFrozenByGlacerus()))
            {
                await _asyncEventPipeline.ProcessEventAsync(new RaidInstanceFinishEvent(raidParty, RaidFinishType.NoLivesLeft), cancellation);
                return;
            }

            monsterEntity.SkillToUse = (short)SkillsVnums.GLACERUS_FREEZE_AFTER;
            monsterEntity.ForceUseSkill = true;
        }

        private static bool IsInSquare(IReadOnlyCollection<(int, Position)> safePosition, IBattleEntity entity)
        {
            foreach ((int _, Position position) in safePosition)
            {
                short entityX = entity.PositionX;
                short entityY = entity.PositionY;

                if (Math.Abs(position.X - entityX) <= 4 && Math.Abs(position.Y - entityY) <= 4)
                {
                    return true;
                }
            }

            return false;
        }
    }