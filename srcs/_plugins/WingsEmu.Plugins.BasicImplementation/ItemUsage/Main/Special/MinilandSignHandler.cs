using System;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class MinilandSignHandler : IItemHandler
{
    private readonly IAsyncEventPipeline _asyncEvent;
    private readonly INpcEntityFactory _npcEntityFactory;

    public MinilandSignHandler(INpcEntityFactory npcEntityFactory, IAsyncEventPipeline asyncEvent)
    {
        _npcEntityFactory = npcEntityFactory;
        _asyncEvent = asyncEvent;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 100 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.HAS_SIGNPOSTS_ENABLED))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT), ChatMessageColorType.Yellow);
            return;
        }

        INpcEntity findSignPost = session.CurrentMapInstance.GetPassiveNpcs().FirstOrDefault(x => x.MinilandOwner != null && x.MinilandOwner.Id == session.PlayerEntity.Id);
        if (findSignPost != null)
        {
            string timeLeft = TimeSpan.FromSeconds(findSignPost.Hp / 5.0).ToString(@"hh\:mm\:ss");
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_SIGNPOST_ALREADY_IN, timeLeft), ChatMessageColorType.Yellow);
            return;
        }

        INpcEntity newSign = _npcEntityFactory.CreateNpc(e.Item.ItemInstance.GameItem.EffectValue, session.CurrentMapInstance, null, new NpcAdditionalData
        {
            MinilandOwner = session.PlayerEntity,
            NpcShouldRespawn = false
        });

        newSign.Direction = 2;
        session.PlayerEntity.SignPostMapInstanceIds.Add(session.CurrentMapInstance.Id);

        await _asyncEvent.ProcessEventAsync(new MapJoinNpcEntityEvent(newSign, session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        await session.RemoveItemFromInventory(item: e.Item);
    }
}