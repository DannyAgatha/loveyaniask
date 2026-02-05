using PhoenixLib.Events;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.SnackFood.Events;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardMagicArmourHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly GameRevivalConfiguration _gameRevivalConfiguration;

    public BCardMagicArmourHandler(IAsyncEventPipeline eventPipeline, GameRevivalConfiguration gameRevivalConfiguration, IBuffFactory buffFactory)
    {
        _eventPipeline = eventPipeline;
        _gameRevivalConfiguration = gameRevivalConfiguration;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.MagicArmour;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.MagicArmour.PreventDamage:
                {
                    if (sender is not IPlayerEntity player)
                    {
                        return;
                    }

                    player.Session.EmitEvent(new AddAdditionalHpMpEvent
                    {
                        Hp = firstData,
                        Mp = 0,
                        MaxHpPercentage = player.MaxHp,
                        MaxMpPercentage = player.MaxMp
                    });
                }
                break;
        }
    }
}