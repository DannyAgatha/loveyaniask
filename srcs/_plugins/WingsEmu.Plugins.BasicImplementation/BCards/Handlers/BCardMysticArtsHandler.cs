using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardMysticArtsHandler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IBuffFactory _buffFactory;

    public BCardMysticArtsHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline eventPipeline, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _eventPipeline = eventPipeline;
        _buffFactory = buffFactory;
    }
    
    public BCardType HandledType => BCardType.MysticArts;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);
        BCardDTO bCard = ctx.BCard;
        
        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.MysticArts.DodgeAndMakeChance:

                if (target.IsSameEntity(sender))
                {
                    return;
                }

                sender.RemoveBuffAsync((int)BuffVnums.SIDESTEP).ConfigureAwait(false).GetAwaiter().GetResult();

                if (sender.IsSucceededChance(firstData))
                {
                    sender.AddBuffAsync(_buffFactory.CreateBuff(secondData, sender)).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            
            case (byte)AdditionalTypes.MysticArts.FullMoonSkillUse when sender is IPlayerEntity player:
                player.Session.RefreshQuicklist();
                break;
            
            case (byte)AdditionalTypes.MysticArts.LotusFlowerSkillUse when sender is IPlayerEntity player:
                player.Session.RefreshQuicklist(true);
                break;
        }
    }
}