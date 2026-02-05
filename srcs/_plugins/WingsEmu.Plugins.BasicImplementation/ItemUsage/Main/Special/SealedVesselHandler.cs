// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class SealedVesselHandler : IItemHandler
{
    private const int MAXIMUM_VESSELS_IN_MINILAND = 30;
    private const int MAXIMUM_VESSELS_IN_NORMAL_MAP = 10;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IDelayManager _delayManager;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IRandomGenerator _randomGenerator;

    private readonly HashSet<short> _vNums = new()
    {
        1386, 1387, 1388, 1389, 1390, 1391, 1392,
        1393, 1394, 1395, 1396, 1397, 1398, 1399,
        1400, 1401, 1402, 1403, 1404, 1405
    };

    public SealedVesselHandler(IRandomGenerator randomGenerator, INpcMonsterManager npcMonsterManager, IAsyncEventPipeline asyncEventPipeline, IGameLanguageService gameLanguage,
        IDelayManager delayManager)
    {
        _randomGenerator = randomGenerator;
        _npcMonsterManager = npcMonsterManager;
        _asyncEventPipeline = asyncEventPipeline;
        _delayManager = delayManager;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 1002 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.CurrentMapInstance.HasMapFlag(MapFlags.HAS_SEALED_VESSELS_DISABLED))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
            session.SendMsg(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), MsgMessageType.Middle);
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && session.CurrentMapInstance.MapInstanceType != MapInstanceType.Miniland)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
            session.SendMsg(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.MapInstance.IsBlockedZone(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY))
        {
            return;
        }

        byte currentVessels = session.PlayerEntity.MapInstance.CurrentVessels();
        if (currentVessels >= (session.PlayerEntity.MapInstance.MapInstanceType == MapInstanceType.Miniland ? MAXIMUM_VESSELS_IN_MINILAND : MAXIMUM_VESSELS_IN_NORMAL_MAP))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INTERACTION_VESSEL_LIMIT_REACHED), MsgMessageType.Middle);
            return;
        }

        if (e.Option == 0)
        {
            DateTime waitUntil = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.SealedVessel);
            session.SendDelay((int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.UsingItem, $"u_i 1 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 1");
            return;
        }

        bool canEquipVehicle = await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.SealedVessel);
        if (!canEquipVehicle)
        {
            return;
        }

        await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.SealedVessel);

        short vNum = _vNums.ElementAt(_randomGenerator.RandomNumber(_vNums.Count));
        IMonsterData npcMonster = _npcMonsterManager.GetNpc(vNum);
        if (npcMonster == null)
        {
            return;
        }

        await session.RemoveItemFromInventory(item: e.Item);

        await _asyncEventPipeline.ProcessEventAsync(new MonsterSummonEvent(session.CurrentMapInstance, new[]
        {
            new ToSummon
            {
                VNum = vNum,
                SpawnCell = new Position(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY),
                IsVesselMonster = true,
                IsMoving = true,
                IsHostile = false
            }
        }));
    }
}