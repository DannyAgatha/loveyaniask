using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class MasterAnglerScrollHandler : IItemUsageByVnumHandler
{
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly IBuffFactory _buffFactory;
    private readonly ISessionManager _sessionManager; 
    
    public MasterAnglerScrollHandler(IBCardEffectHandlerContainer bCardEffectHandlerContainer, IBuffFactory buffFactory, ISessionManager sessionManager)
    {
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
        _buffFactory = buffFactory;
        _sessionManager = sessionManager;
    }

    public long[] Vnums =>
    [
        (long)ItemVnums.MASTER_ANGLER_SCROLL
    ];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.HasBuff(BuffVnums.MASTER_ANGLER_ENERGY))
        {
            return;
        }
        
        foreach (IClientSession activeSession in _sessionManager.Sessions)
        {
            activeSession?.SendSayi2(EntityType.Mate, ChatMessageColorType.Yellow, Game18NConstString.MasterAngler, I18NArgumentType.ItemScroll, session.PlayerEntity.Name);
        }
        
        await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateOneHourBuff(session.PlayerEntity, (short)BuffVnums.MASTER_ANGLER_ENERGY, BuffFlag.BIG_AND_KEEP_ON_LOGOUT));
        await session.RemoveItemFromInventory(item: e.Item);
    }
}