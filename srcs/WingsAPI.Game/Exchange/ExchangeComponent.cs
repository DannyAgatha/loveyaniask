namespace WingsEmu.Game.Exchange;

public class ExchangeComponent : IExchangeComponent
{
    private PlayerExchange _exchange;

    public ExchangeComponent() => _exchange = null;

    public void SetExchange(PlayerExchange exchange)
    {
        if (_exchange != null)
        {
            return;
        }

        _exchange = exchange;
    }

    public void RemoveExchange()
    {
        _exchange = null;
    }

    public PlayerExchange GetExchange() => _exchange;

    public bool IsInExchange() => _exchange != null;

    public long GetTargetId() => _exchange?.TargetId ?? 0;
}