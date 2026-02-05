// NosEmu
// 


using System;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardChangingPlaceHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;

    public BCardChangingPlaceHandler(IBuffFactory buffFactory)
    {
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.ChangingPlace;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity target = ctx.Target;
        IBattleEntity sender = ctx.Sender;
        byte subType = ctx.BCard.SubType;

        switch ((AdditionalTypes.ChangingPlace)subType)
        {
            case AdditionalTypes.ChangingPlace.ReplaceTargetPosition:
                {
                    if (target is not IPlayerEntity playerEntity)
                    {
                        return;
                    }

                    playerEntity.SkillComponent.NeliaId = sender.Id;

                    playerEntity.SkillComponent.NeliaPosition = sender.Position;
                    playerEntity.SkillComponent.PlayerPosition = target.Position;

                    sender.ChangePosition(playerEntity.SkillComponent.PlayerPosition);
                    sender.TeleportOnMap(sender.PositionX, sender.PositionY);
                    playerEntity.ChangePosition(playerEntity.SkillComponent.NeliaPosition);
                    playerEntity.TeleportOnMap(playerEntity.PositionX, playerEntity.PositionY);
                }
                
                break;
        }
    }
}