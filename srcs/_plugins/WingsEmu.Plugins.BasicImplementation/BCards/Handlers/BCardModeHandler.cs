using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardModeHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.Mode;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;

        if (sender is not IMonsterEntity monsterEntity)
        {
            return;
        }

        BCardDTO bCardDto = ctx.BCard;
        int firstData = bCardDto.FirstDataValue(sender.Level);

        switch (bCardDto.SubType)
        {
            case (byte)AdditionalTypes.Mode.Range:

                monsterEntity.BasicSkill.Range = (byte)firstData;

                break;
            case (byte)AdditionalTypes.Mode.ReturnRange:

                monsterEntity.BasicSkill.Range = monsterEntity.BasicRange;

                break;
            case (byte)AdditionalTypes.Mode.AttackTimeIncreased:

                monsterEntity.BasicSkill.Cooldown = (short)(monsterEntity.BasicSkill.Cooldown + firstData);

                break;
            case (byte)AdditionalTypes.Mode.AttackTimeDecreased:

                monsterEntity.BasicSkill.Cooldown = (short)(monsterEntity.BasicSkill.Cooldown - firstData);

                break;
        }
    }
}