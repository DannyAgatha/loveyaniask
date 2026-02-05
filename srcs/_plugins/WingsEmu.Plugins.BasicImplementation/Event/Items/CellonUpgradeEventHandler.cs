using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Items;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Cellons;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class CellonUpgradeEventHandler : IAsyncEventProcessor<CellonUpgradeEvent>
{
    private readonly ICellonGenerationAlgorithm _cellonGenerationAlgorithm;
    private readonly CellonSystemConfiguration _cellonUpgradeConfiguration;
    private readonly IGameLanguageService _language;
    private readonly IRandomGenerator _randomGenerator;

    public CellonUpgradeEventHandler(IGameLanguageService language, ICellonGenerationAlgorithm cellonGenerationAlgorithm, CellonSystemConfiguration cellonUpgradeConfiguration,
        IRandomGenerator randomGenerator)
    {
        _language = language;
        _cellonGenerationAlgorithm = cellonGenerationAlgorithm;
        _cellonUpgradeConfiguration = cellonUpgradeConfiguration;
        _randomGenerator = randomGenerator;
    }

    public async Task HandleAsync(CellonUpgradeEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        InventoryItem cellonItem = e.Cellon;
        GameItemInstance cellon = cellonItem.ItemInstance;
        GameItemInstance upgradableItem = e.UpgradableItem;

        if (cellon.GameItem.EffectValue > upgradableItem.GameItem.MaxCellonLvl)
        {
            session.SendMsg(_language.GetLanguage(GameDialogKey.CELLON_SHOUTMESSAGE_LEVEL_TOO_HIGH, session.UserLanguage), MsgMessageType.Middle);
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }

        if (upgradableItem.GameItem.MaxCellon <= upgradableItem.EquipmentOptions?.Count)
        {
            session.SendMsg(_language.GetLanguage(GameDialogKey.CELLON_SHOUTMESSAGE_OPTIONS_FULL, session.UserLanguage), MsgMessageType.Middle);
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }

        CellonPossibilities mats = _cellonUpgradeConfiguration.Options.FirstOrDefault(s => s.CellonLevel == cellon.GameItem.EffectValue);
        if (mats == default)
        {
            return;
        }

        int gold = mats.Price;
        if (!session.HasEnoughGold(gold))
        {
            session.SendMsg(_language.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage), MsgMessageType.Middle);
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }

        // REMOVE ITEMS TO USE
        session.PlayerEntity.RemoveGold(gold);
        await session.RemoveItemFromInventory(item: cellonItem);
        
        await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.SpendXGoldToNpc, gold));

        // roll chance
        CellonChances tmp = _cellonUpgradeConfiguration.ChancesToSuccess.FirstOrDefault(s => s.CellonAmount == upgradableItem.Cellon);

        if (tmp == null)
        {
            return;
        }

        int roll = _randomGenerator.RandomNumber((int)Math.Floor(tmp.SuccessChance));
        if (roll >= tmp.SuccessChance)
        {
            session.SendMsg(_language.GetLanguage(GameDialogKey.CELLON_SHOUTMESSAGE_FAIL, session.UserLanguage), MsgMessageType.Middle);
            session.SendShopEndPacket(ShopEndType.Npc);
            return;
        }

        // GENERATE OPTION
        EquipmentOptionDTO option = _cellonGenerationAlgorithm.GenerateOption(cellon.GameItem.EffectValue);

        upgradableItem.EquipmentOptions ??= new List<EquipmentOptionDTO>();

        // FAIL
        if (option == null || upgradableItem.EquipmentOptions.Any(s => s.Type == option.Type))
        {
            session.SendMsg(_language.GetLanguage(GameDialogKey.CELLON_SHOUTMESSAGE_FAIL, session.UserLanguage), MsgMessageType.Middle);
            session.SendShopEndPacket(ShopEndType.Npc);
            await session.EmitEventAsync(new CellonUpgradedEvent
            {
                Item = upgradableItem,
                CellonVnum = cellon.ItemVNum,
                Succeed = false
            });
            return;
        }

        // SUCCESS
        upgradableItem.EquipmentOptions.Add(option);
        upgradableItem.Cellon++;
        session.SendMsg(_language.GetLanguage(GameDialogKey.CELLON_SHOUTMESSAGE_SUCCESS, session.UserLanguage), MsgMessageType.Middle);
        session.SendShopEndPacket(ShopEndType.Npc);

        await session.EmitEventAsync(new CellonUpgradedEvent
        {
            Item = upgradableItem,
            CellonVnum = cellon.ItemVNum,
            Succeed = true
        });
    }
}