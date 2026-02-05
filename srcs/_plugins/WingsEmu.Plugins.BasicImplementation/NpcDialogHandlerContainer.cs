using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations;

public class NpcDialogHandlerContainer : INpcDialogHandlerContainer
{
    private readonly Dictionary<NpcRunType, INpcDialogAsyncHandler> _handlers;

    public NpcDialogHandlerContainer() => _handlers = new Dictionary<NpcRunType, INpcDialogAsyncHandler>();

    public void Register(INpcDialogAsyncHandler handler)
    {
        foreach (NpcRunType npcRunType in handler.NpcRunTypes)
        {
            if (_handlers.ContainsKey(npcRunType))
            {
                continue;
            }

            _handlers.Add(npcRunType, handler);
            Log.Debug($"[NPC_DIALOG][REGISTER_HANDLER] NPC_RUN_TYPE: {npcRunType.ToString()} REGISTERED!");
        }
    }

    public void Unregister(INpcDialogAsyncHandler handler)
    {
        foreach (NpcRunType npcRunType in handler.NpcRunTypes)
        {
            Log.Debug($"[NPC_DIALOG][UNREGISTER_HANDLER] NPC_RUN_TYPE: {handler.NpcRunTypes} UNREGISTERED!");
            _handlers.Remove(npcRunType);
        }
    }

    public void Execute(IClientSession session, NpcDialogEvent e)
    {
        if (session.PlayerEntity.IsInExchange())
            return;
        if (session.PlayerEntity.HasShopOpened)
            return;
        if (e.NpcRunType < 0)
            return;
        ExecuteAsync(session, e).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task ExecuteAsync(IClientSession session, NpcDialogEvent e)
    {
        if (!_handlers.TryGetValue(e.NpcRunType, out INpcDialogAsyncHandler handler))
        {
            Log.Debug($"[HANDLER_NOT_FOUND] NPC_RUN_TYPE: {e.NpcRunType}");
            return;
        }

        Log.Debug($"[NPC_DIALOG][HANDLER] Handling: {e.NpcRunType}");
        await handler.Execute(session, e);
    }
}