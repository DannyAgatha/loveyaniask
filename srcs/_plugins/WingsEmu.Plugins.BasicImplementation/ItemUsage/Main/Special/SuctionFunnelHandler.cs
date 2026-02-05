using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class SuctionFunnelHandler : IItemHandler
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;

    private readonly IGameLanguageService _gameLanguage;

    public SuctionFunnelHandler(IGameLanguageService gameLanguage, IAsyncEventPipeline eventPipeline, IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _gameLanguage = gameLanguage;
        _eventPipeline = eventPipeline;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 400 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity == null)
        {
            return;
        }

        IMonsterEntity kenko = session.CurrentMapInstance?.GetMonsterById(session.PlayerEntity.LastEntity.Item2);
        if (kenko == null)
        {
            return;
        }

        if (kenko.MonsterVNum > (short)MonsterVnum.ELITE_KENKO_RAIDER || kenko.MonsterVNum < (short)MonsterVnum.ROOKIE_KENKO_SWORDSMAN)
        {
            return;
        }

        if (kenko.GetHpPercentage() >= 20) // check if HP is red
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.QUEST_CHATMESSAGE_KENKO_IS_TOO_STRONG, session.UserLanguage), ChatMessageColorType.Red);
            return;
        }

        await _eventPipeline.ProcessEventAsync(new MonsterDeathEvent(kenko));
        kenko.BroadcastDie();
        kenko.MapInstance.RemoveMonster(kenko);

        // Adds the bead and removes the Suction Funnel
        GameItemInstance kenkoBead = _gameItemInstanceFactory.CreateItem((int)ItemVnums.KENKO_BEAD);
        await session.AddNewItemToInventory(kenkoBead, sendGiftIsFull: true);
        await session.RemoveItemFromInventory(item: e.Item);
    }
}