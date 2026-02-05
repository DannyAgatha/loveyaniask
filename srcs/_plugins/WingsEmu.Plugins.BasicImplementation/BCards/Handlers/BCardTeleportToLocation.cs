using System;
using System.Collections.Generic;
using WingsAPI.Communication.Sessions.Model;
using WingsEmu.Game;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardTeleportToLocation : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.Other;

    public BCardTeleportToLocation(IRandomGenerator randomGenerator)
    {
        _randomGenerator = randomGenerator;
    }

    private readonly IRandomGenerator _randomGenerator;
    
    private readonly Dictionary<int, int[]> _fullnessByBearing = new()
    {
        [1] = [0, 25000],
        [2] = [25001, 75000],
        [3] = [75001, 425000],
    };

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData =  ctx.BCard.SecondDataValue(sender.Level);
        Position position = ctx.Position;

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.Other.TeleportToLocation:
                sender.ChangePosition(position);
                sender.TeleportOnMap(sender.PositionX, sender.PositionY);
                break;
            
            case (byte)AdditionalTypes.Other.FoodValueIncreasedOrReducedBy when sender is IPlayerEntity player:
                
                if (!_fullnessByBearing.TryGetValue(secondData, out int[] range))
                {
                    player.Session.SendSayi(ChatMessageColorType.PlayerSay, Game18NConstString.FullMorsel, 4);
                    return;
                }

                int maxPossibleIncrease = range[1] - player.FoodValue;
                int adjustedIncrease = Math.Min(firstData, maxPossibleIncrease);

                if (adjustedIncrease <= 0)
                {
                    return;
                }
                
                if (_randomGenerator.RandomNumber() < 20)
                {
                    player.Session.EmitEvent(new IncreaseFoodValueEvent(adjustedIncrease));
                    player.Session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.PositiveFullnessPoints, 4, adjustedIncrease);
                    return;
                }
                player.Session.EmitEvent(new DecreaseFoodValueEvent(firstData));
                player.Session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.NegativeFullNessPoints, 4, firstData);
                
                break;
        }
    }
}