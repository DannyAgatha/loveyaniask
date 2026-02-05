using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids.Handlers;

public class RaidProcessGlacerusPreFreezeEventHandler : IAsyncEventProcessor<BattleExecuteSkillEvent>
{
    private static readonly EffectType[] SafePositions = { EffectType.GlacerusMonk, EffectType.GlacerusScout, EffectType.GlacerusPoseidon };
    private readonly IRaidManager _raidManager;
    private readonly IRandomGenerator _randomGenerator;

    public RaidProcessGlacerusPreFreezeEventHandler(IRaidManager raidManager, IRandomGenerator randomGenerator)
    {
        _raidManager = raidManager;
        _randomGenerator = randomGenerator;
    }

    public async Task HandleAsync(BattleExecuteSkillEvent e, CancellationToken cancellation)
    {
        IBattleEntity glacerus = e.Entity;
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

        if ((DateTime.Now - raidParty.StartTime).TotalSeconds < 10)
        {
            return;
        }

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
        
        if (raidSubInstance.LastGlacerusFreezeTime != default && 
            (DateTime.Now - raidSubInstance.LastGlacerusFreezeTime).TotalSeconds < 30)
        {
            return;
        }
        
        raidSubInstance.LastGlacerusFreezeTime = DateTime.Now;
        
        // TO DO : rework
        List<(int, Position)> safePositions = new();

        const short minX = 21;
        const short maxX = 48;

        const short minY = 14;
        const short maxY = 42;

        var safePositionsEffects = SafePositions.ToList();

        string[] safePositionsPackets = new string[3];

        int id = 0;
        while (safePositions.Count < 3)
        {
            short x = (short)_randomGenerator.RandomNumber(minX, maxX);
            short y = (short)_randomGenerator.RandomNumber(minY, maxY);

            Position position = new(x, y);

            if (safePositions.Any(pos => pos.Item2.X == x && pos.Item2.Y == y))
            {
                continue;
            }

            int effectIndex = _randomGenerator.RandomNumber(0, safePositionsEffects.Count);
            EffectType effectType = safePositionsEffects[effectIndex];
            safePositionsEffects.Remove(effectType);

            string packet = UiPacketExtension.GenerateEffectGround(id, effectType, x, y, false);
            safePositionsPackets[id] = packet;
            id++;

            safePositions.Add(((short)effectType, position));
        }

        raidSubInstance.GlacerusSafePositions = safePositions;

        foreach (IClientSession session in glacerus.MapInstance.Sessions)
        {
            session.SendPackets(safePositionsPackets);
            session.SendMsg(session.GetLanguage("GLACERUS_CHANNELS_COLD"), MsgMessageType.Middle);
            session.SendChatMessage(session.GetLanguage("GLACERUS_CHANNELS_COLD"), ChatMessageColorType.Red);

            session.SendMsg(session.GetLanguage("GLACERUS_ELISIA_SAVE"), MsgMessageType.SmallMiddle);
            session.SendChatMessage(session.GetLanguage("GLACERUS_ELISIA_SAVE"), ChatMessageColorType.LightPurple);
        }
    }
}