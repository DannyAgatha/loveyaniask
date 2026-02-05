using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act4.Event;

public class Act4PutFlagEventHandler : IAsyncEventProcessor<Act4PutFlagEvent>
{
    private readonly IAct4FlagManager _act4FlagManager;
    private readonly SerializableGameServer _gameServer;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly ISessionManager _sessionManager;

    public Act4PutFlagEventHandler(IAct4FlagManager act4FlagManager, IMonsterEntityFactory monsterEntityFactory, ISessionManager sessionManager, SerializableGameServer gameServer)
    {
        _act4FlagManager = act4FlagManager;
        _monsterEntityFactory = monsterEntityFactory;
        _sessionManager = sessionManager;
        _gameServer = gameServer;
    }

    public async Task HandleAsync(Act4PutFlagEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        GameItemInstance item = e.InventoryItem.ItemInstance;
        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return;
        }

        if (_gameServer.ChannelType != GameChannelType.ACT_4)
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            return;
        }

        switch (item.ItemVNum)
        {
            case (short)ItemVnums.ANGEL_BASE_FLAG when session.PlayerEntity.Faction != FactionType.Angel:
            case (short)ItemVnums.DEMON_BASE_FLAG when session.PlayerEntity.Faction != FactionType.Demon:
                return;
        }

        if (session.CurrentMapInstance.MapVnum is (short)MapIds.ACT4_ANGEL_CITADEL or (short)MapIds.ACT4_DEMON_CITADEL)
        {
            return;
        }

        if (session.PlayerEntity.Level < 70)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_NOT_REQUIERED_LEVEL), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.Reput < 100000)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_NOT_ENOUGH_REPUT), MsgMessageType.Middle);
            return;
        }

        FactionType faction = session.PlayerEntity.Faction;

        MapLocation map = faction == FactionType.Angel ? _act4FlagManager.AngelFlag : _act4FlagManager.DemonFlag;
        if (map != null)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.ACT4_SHOUTMESSAGE_FLAG_ALREADY_IN), MsgMessageType.Middle);
            return;
        }

        MapLocation placement = new()
        {
            MapInstanceId = session.CurrentMapInstance.Id,
            X = session.PlayerEntity.PositionX,
            Y = session.PlayerEntity.PositionY
        };

        switch (faction)
        {
            case FactionType.Angel:
                _act4FlagManager.SetAngelFlag(placement);
                break;
            case FactionType.Demon:
                _act4FlagManager.SetDemonFlag(placement);
                break;
            default:
                return;
        }

        IMonsterEntity monster = _monsterEntityFactory.CreateMonster(e.InventoryItem.ItemInstance.GameItem.Data[2], session.CurrentMapInstance, new MonsterEntityBuilder
        {
            IsHostile = false,
            IsWalkingAround = false,
            FactionType = faction
        });

        GameDialogKey message = faction == FactionType.Angel ? GameDialogKey.ACT4_SHOUTMESSAGE_FLAG_PLACED_ANGEL : GameDialogKey.ACT4_SHOUTMESSAGE_FLAG_PLACED_DEMON;

        _sessionManager.Broadcast(x => x.GenerateMsgPacket(x.GetLanguage(message), MsgMessageType.Middle));
        await monster.EmitEventAsync(new MapJoinMonsterEntityEvent(monster, placement.X, placement.Y));
        await session.RemoveItemFromInventory(item: e.InventoryItem);
    }
}