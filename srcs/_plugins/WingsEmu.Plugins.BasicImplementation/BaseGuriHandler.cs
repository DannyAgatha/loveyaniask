using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations;

public class BaseGuriHandler : IGuriHandlerContainer
{
    private readonly Dictionary<long, IGuriHandler> _handlersByDialogId;

    public BaseGuriHandler() => _handlersByDialogId = new Dictionary<long, IGuriHandler>();

    public void Register(IGuriHandler handler)
    {
        if (_handlersByDialogId.ContainsKey(handler.GuriEffectId))
        {
            return;
        }

        Log.Debug($"[GURI_HANDLER][REGISTER] GURI_EFFECT : {handler.GuriEffectId} REGISTERED !");
        _handlersByDialogId.Add(handler.GuriEffectId, handler);
    }

    public void Unregister(long guriEffectId)
    {
        Log.Debug($"[GURI_HANDLER][UNREGISTER] GURI_EFFECT : {guriEffectId} UNREGISTERED !");
        _handlersByDialogId.Remove(guriEffectId);
    }

    public void Handle(IClientSession player, GuriEvent args)
    {
        if (player.PlayerEntity.IsInExchange())
            return;
        if (player.PlayerEntity.HasShopOpened)
            return;
        if (args.Packet.Length < 0)
            return;
        HandleAsync(player, args).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task HandleAsync(IClientSession player, GuriEvent args)
    {
        if (!_handlersByDialogId.TryGetValue(args.EffectId, out IGuriHandler handler))
        {
            Log.Debug($"[GURI_HANDLER] GURI_EFFECT : {args.EffectId} ");
            return;
        }

        Log.Debug($"[GURI_HANDLER][HANDLING] : {args.EffectId} ");
        await handler.ExecuteAsync(player, args);
    }
}