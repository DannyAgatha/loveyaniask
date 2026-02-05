using System.Collections.Generic;
using System.Text;
using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Entities;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.ServerPackets.Battle;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardCalvinasHandler : IBCardEffectAsyncHandler
{
    private readonly Act4DungeonsConfiguration _act4DungeonsConfiguration;
    private readonly IDungeonManager _dungeonManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IAsyncEventPipeline _eventPipeline;

    public BCardCalvinasHandler(IDungeonManager dungeonManager, Act4DungeonsConfiguration act4DungeonsConfiguration, IRandomGenerator randomGenerator, IAsyncEventPipeline eventPipeline)
    {
        _dungeonManager = dungeonManager;
        _act4DungeonsConfiguration = act4DungeonsConfiguration;
        _randomGenerator = randomGenerator;
        _eventPipeline = eventPipeline;
    }

    public BCardType HandledType => BCardType.LordCalvinas;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        SkillInfo skill = ctx.Skill;
        SuPacketHitMode hitMode = ctx.HitMode;
        int damageDealt = ctx.DamageDealt;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData =  ctx.BCard.SecondDataValue(sender.Level);
        byte subType = ctx.BCard.SubType;

        switch (subType)
        {
            case (byte)AdditionalTypes.LordCalvinas.InflictDamageAtLocation:
                var dragonCord = new StringBuilder();
                List<CalvinasDragon> calvinasDragons = [];

                int amountOfDragons = _randomGenerator.RandomNumber(1, 3);

                for (int i = 0; i < amountOfDragons; i++)
                {
                    int at = _randomGenerator.RandomNumber(0, 11);
                    int axis = _randomGenerator.RandomNumber(0, 2);

                    var newDragon = new CalvinasDragon
                    {
                        Axis = axis == 0 ? CoordType.X : CoordType.Y,
                        Size = 3,
                        At = (short)(at * 5),
                        Start = -50,
                        End = 400
                    };

                    calvinasDragons.Add(newDragon);
                }

                foreach (CalvinasDragon dragon in calvinasDragons)
                {
                    if (dragon.Axis == CoordType.X)
                    {
                        dragonCord.Append($"{dragon.Start} {dragon.At} {dragon.End} {dragon.At} ");
                    }
                    else
                    {
                        dragonCord.Append($"{dragon.At} {dragon.Start} {dragon.At} {dragon.End} ");
                    }
                }

                if (calvinasDragons.Count == 1)
                {
                    dragonCord.Append("0 0 0 0");
                }

                sender.MapInstance.Broadcast(sender.GenerateDragonPacket((byte)calvinasDragons.Count) + dragonCord);
                _dungeonManager.AddCalvinasDragons(sender.MapInstance.Id, new CalvinasState
                {
                    CalvinasDragonsList = calvinasDragons,
                    CastTime = sender.GenerateSkillCastTime(ctx.Skill)
                });
                break;
        }
    }
}