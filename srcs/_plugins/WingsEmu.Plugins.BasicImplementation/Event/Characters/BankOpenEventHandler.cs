using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class BankOpenEventHandler : IAsyncEventProcessor<BankOpenEvent>
{
    private readonly IBankReputationConfiguration _bankReputationConfiguration;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;

    public BankOpenEventHandler(IGameLanguageService gameLanguage, IReputationConfiguration reputationConfiguration, IBankReputationConfiguration bankReputationConfiguration,
        IRankingManager rankingManager)
    {
        _gameLanguage = gameLanguage;
        _reputationConfiguration = reputationConfiguration;
        _bankReputationConfiguration = bankReputationConfiguration;
        _rankingManager = rankingManager;
    }

    public async Task HandleAsync(BankOpenEvent e, CancellationToken cancellation)
    {
        long? npcId = e.NpcId;
        InventoryItem bankCard = e.BankCard;
        IClientSession session = e.Sender;

        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            return;
        }

        if (npcId.HasValue)
        {
            INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(npcId.Value);
            if (npcEntity?.ShopNpc == null)
            {
                return;
            }

            if (npcEntity.ShopNpc.ShopType != (byte)NpcShopType.BANK)
            {
                return;
            }
        }
        else
        {
            if (bankCard == null)
            {
                return;
            }

            await session.RemoveItemFromInventory(item: bankCard);
        }
        
        session.SendGbPacket(BankType.OpenBank, _reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
        session.SendSMemoI2(SmemoType.BankInfo, Game18NConstString.WelcomeToQuarry, 2, session.PlayerEntity.Name);
        session.PlayerEntity.IsBankOpen = true;
    }
}