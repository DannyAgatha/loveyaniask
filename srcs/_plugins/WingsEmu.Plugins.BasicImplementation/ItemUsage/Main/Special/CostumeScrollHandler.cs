using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

/// <summary>
///     This handle is use for Halloween, Winter & Bushtail costume scroll
/// </summary>
public class CostumeScrollHandler : IItemHandler
{
    private readonly ICostumeScrollConfiguration _costumeScrollConfiguration;
    private readonly IDelayManager _delayManager;

    private readonly IGameLanguageService _languageService;
    private readonly IRandomGenerator _randomGenerator;

    public CostumeScrollHandler(IGameLanguageService languageService, IDelayManager delayManager, IRandomGenerator randomGenerator, ICostumeScrollConfiguration costumeScrollConfiguration)
    {
        _languageService = languageService;
        _delayManager = delayManager;
        _randomGenerator = randomGenerator;
        _costumeScrollConfiguration = costumeScrollConfiguration;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 1001 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IReadOnlyList<short> morphs = _costumeScrollConfiguration.GetScrollMorphs((short)e.Item.ItemInstance.ItemVNum);

        if (morphs == null || morphs.Count == 0)
        {
            return;
        }

        if (session.PlayerEntity.UseSp)
        {
            session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        if (e.Option == 0)
        {
            if (session.PlayerEntity.IsMorphed)
            {
                session.PlayerEntity.IsMorphed = false;
                session.PlayerEntity.Morph = 0;

                session.BroadcastCMode();
                return;
            }

            DateTime waitUntil = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.MorphScroll);
            session.SendDelay((int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.Transforming,
                $"u_i 1 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 2");
            return;
        }

        bool canUseScroll = await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.MorphScroll);

        if (!canUseScroll)
        {
            return;
        }

        await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.MorphScroll);

        session.PlayerEntity.IsMorphed = true;
        session.PlayerEntity.Morph = morphs[_randomGenerator.RandomNumber(0, morphs.Count)] + 1000;

        session.BroadcastCMode();
        session.BroadcastEffect((int)EffectType.Transform, new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        await session.RemoveItemFromInventory(item: e.Item);
    }
}