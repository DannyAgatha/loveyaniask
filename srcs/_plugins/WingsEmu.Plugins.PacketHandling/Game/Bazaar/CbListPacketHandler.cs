using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Bazaar;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Bazaar.Configuration;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Bazaar;

public class CbListPacketHandler : GenericGamePacketHandlerBase<CbListPacket>
{
    private readonly BazaarConfiguration _bazaarConfiguration;
    private readonly IGameLanguageService _languageService;

    public CbListPacketHandler(IGameLanguageService languageService, BazaarConfiguration bazaarConfiguration)
    {
        _languageService = languageService;
        _bazaarConfiguration = bazaarConfiguration;
    }

    protected override async Task HandlePacketAsync(IClientSession session, CbListPacket packet)
    {
        DateTime currentDate = DateTime.UtcNow;
        if (session.PlayerEntity.LastBuySearchBazaarRefresh > currentDate)
        {
            return;
        }

        session.PlayerEntity.LastBuySearchBazaarRefresh = currentDate.AddSeconds(_bazaarConfiguration.DelayServerBetweenRequestsInSecs);

        if (packet.ItemVNumFilter == null)
        {
            return;
        }

        string[] splitedString = packet.ItemVNumFilter.Split(' ');

        List<int> list = null;

        for (int i = 0; i < splitedString.Length; i++)
        {
            short value = Convert.ToInt16(splitedString[i]);
            if (i == 0)
            {
                i += value + 1;
                continue;
            }

            list ??= new List<int>();
            list.Add(value);
        }

        await session.EmitEventAsync(new BazaarSearchItemsEvent(packet.Index, packet.CategoryFilterType, packet.SubTypeFilter, packet.LevelFilter, packet.RareFilter, packet.UpgradeFilter,
            packet.OrderFilter, list));
    }
}