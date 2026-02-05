using PhoenixLib.Events;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Teacher;

public class NosMateTrainerHandler : IItemUsageByVnumHandler
{
    private const int MAX_DOLLS = 10;
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameLanguageService _gameLanguage;
    private readonly INpcMonsterManager _monsterManager;

    public NosMateTrainerHandler(IGameLanguageService gameLanguage, INpcMonsterManager monsterManager, IAsyncEventPipeline eventPipeline, IBCardEffectHandlerContainer bCardEffectHandlerContainer)
    {
        _gameLanguage = gameLanguage;
        _monsterManager = monsterManager;
        _eventPipeline = eventPipeline;
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
    }

    public long[] Vnums => new long[] { 2079, 2129, 2321, 2323, 2328, 2477, 10017 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        int monsterVnum = e.Item.ItemInstance.GameItem.EffectValue;

        IPlayerEntity character = session.PlayerEntity;
        IMonsterData monster = _monsterManager.GetNpc(monsterVnum);
        IGameItem gameItem = e.Item.ItemInstance.GameItem;

        if (monster == null)
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_MINILAND_MAP))
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_ONLY_IN_MINILAND, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!session.PlayerEntity.IsInMateDollZone())
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.DOLL_SHOUTMESSAGE_NOT_IN_ZONE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        int dolls = session.CurrentMapInstance.GetAliveMonsters(x => x != null && x.IsAlive() &&
        x.SummonerId == session.PlayerEntity.Id && x.SummonerType == VisualType.Player && x.IsMateTrainer).Count;

        if (dolls >= MAX_DOLLS)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.DOLL_SHOUTMESSAGE_DOLLS_LIMIT, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        foreach (BCardDTO bCard in gameItem.BCards)
        {
            _bCardEffectHandlerContainer.Execute(character, character, bCard);
        }

        await session.RemoveItemFromInventory(item: e.Item);
    }
}