using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class SpWingHandler : IItemHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameLanguageService _languageService;
    private readonly ISpWingConfiguration _spWingConfiguration;

    public SpWingHandler(IGameLanguageService languageService, ISpWingConfiguration spWingConfiguration, IAsyncEventPipeline eventPipeline, IBuffFactory buffFactory)
    {
        _languageService = languageService;
        _spWingConfiguration = spWingConfiguration;
        _eventPipeline = eventPipeline;
        _buffFactory = buffFactory;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 650 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        if (!session.PlayerEntity.UseSp || session.PlayerEntity.Specialist == null)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_SPECIALIST_CARD, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.Specialist.Design == e.Item.ItemInstance.GameItem.EffectValue && session.PlayerEntity.MorphUpgrade2 == e.Item.ItemInstance.GameItem.EffectValue)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_SAME_SP_WINGS_ALREADY_SET, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.Specialist.Upgrade == 0)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.INTERACTION_SHOUTMESSAGE_NEED_SP_UPGRADE_FOR_WINGS, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (e.Option == 0)
        {
            session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 3",
                _languageService.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_CHANGE_SP_WINGS, session.UserLanguage));
            return;
        }

        SpWingInfo newWingInfo = _spWingConfiguration.GetSpWingInfo(e.Item.ItemInstance.GameItem.EffectValue);
        SpWingInfo oldWingInfo = _spWingConfiguration.GetSpWingInfo(session.PlayerEntity.Specialist.WingsDesign);

        if (oldWingInfo != null)
        {
            foreach (WingBuff buff in oldWingInfo.Buffs)
            {
                Buff wingBuff = session.PlayerEntity.BuffComponent.GetBuff(buff.BuffId);
                await session.PlayerEntity.RemoveBuffAsync(buff.IsPermanent, wingBuff);
            }
        }

        session.PlayerEntity.Specialist.WingsDesign = (byte)e.Item.ItemInstance.GameItem.EffectValue;
        session.PlayerEntity.Specialist.Design = (byte)e.Item.ItemInstance.GameItem.EffectValue;
        session.PlayerEntity.MorphUpgrade2 = e.Item.ItemInstance.GameItem.EffectValue;
        session.PlayerEntity.Specialist.EquippedWingsVNum = (short)e.Item.ItemInstance.ItemVNum;

        session.BroadcastCMode();
        session.RefreshStat();
        session.RefreshStatChar();

        if (newWingInfo != null)
        {
            foreach (WingBuff buff in newWingInfo.Buffs)
            {
                await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff(buff.BuffId, session.PlayerEntity, buff.IsPermanent ? BuffFlag.NO_DURATION : BuffFlag.NORMAL));
            }
        }

        await session.RemoveItemFromInventory(item: e.Item);
    }
}