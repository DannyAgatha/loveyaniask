namespace WingsEmu.Game.Buffs;

/// <summary>
///     Handlers of an BCardEventContext
/// </summary>
public interface IBCardEffectAsyncHandler
{
    BCardType HandledType { get; }

    void Execute(IBCardEffectContext ctx);
}