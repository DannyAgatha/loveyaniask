using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Core.Extensions;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Ship;
using WingsEmu.Game.Ship.Configuration;
using WingsEmu.Game.Ship.Event;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Ship.Event;

public class ShipEnterEventHandler : IAsyncEventProcessor<ShipEnterEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IShipManager _shipManager;

    public ShipEnterEventHandler(IShipManager shipManager, IGameLanguageService languageService, IRandomGenerator randomGenerator)
    {
        _shipManager = shipManager;
        _languageService = languageService;
        _randomGenerator = randomGenerator;
    }

    public async Task HandleAsync(ShipEnterEvent e, CancellationToken cancellation)
    {
        ShipInstance ship = _shipManager.GetShip(e.ShipType);
        if (ship?.Configuration == null)
        {
            return;
        }

        TimeSpan timeLeft = ship.LastDeparture + ship.Configuration.Departure - DateTime.UtcNow;
        if (timeLeft.TotalSeconds < 10)
        {
            e.Sender.SendMsg(e.Sender.GetLanguage(GameDialogKey.SHIP_SHOUTMESSAGE_COOLDOWN), MsgMessageType.Middle);
            return;
        }

        long baseToRemove = ship.Configuration.ShipCost;
        short toRemove = e.Sender.PlayerEntity.Family?.UpgradeValues.GetOrDefault(FamilyUpgradeType.DECREASE_SHIP_TP_COST).Item1 ?? 0;
        long amountToRemove = (long)(baseToRemove * (toRemove * 0.01));
        baseToRemove -= amountToRemove;

        if (e.Sender.PlayerEntity.Gold < baseToRemove)
        {
            e.Sender.SendInformationChatMessage(_languageService.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, e.Sender.UserLanguage));
            return;
        }

        if (e.Sender.PlayerEntity.Level < ship.Configuration.ShipLevelRestriction)
        {
            e.Sender.SendInformationChatMessage(_languageService.GetLanguageFormat(GameDialogKey.SHIP_CHATMESSAGE_LEVEL_REQ, e.Sender.UserLanguage, ship.Configuration.ShipLevelRestriction));
            return;
        }

        short rndX = (short)_randomGenerator.RandomNumber(ship.Configuration.ShipMapX.Minimum, ship.Configuration.ShipMapX.Maximum);
        short rndY = (short)_randomGenerator.RandomNumber(ship.Configuration.ShipMapY.Minimum, ship.Configuration.ShipMapY.Maximum);

        if (ship.MapInstance.IsBlockedZone(rndX, rndY))
        {
            rndX = ship.Configuration.ShipMapX.Minimum;
            rndY = ship.Configuration.ShipMapY.Minimum;
        }

        e.Sender.ChangeMap(ship.MapInstance.Id, rndX, rndY);
        if (ship.DepartureWarnings.Count < 1)
        {
            return;
        }

        TimeSpan currentWarning = ship.DepartureWarnings.First();
        TimeSpan shipLeft = ship.Configuration.Departure - currentWarning;
        bool isSeconds = timeLeft.TotalMinutes < 1;


        if (e.ShipType == ShipType.Act7)
        {
            e.Sender.GenerateMsgiPacket(
                MessageType.Default,
                Game18NConstString.TransportToMoritiusShortly,
                4,
                isSeconds ? timeLeft.Seconds : timeLeft.Minutes);
        }
        else
        {
            e.Sender.SendMsg(e.Sender.GetLanguageFormat(GameDialogKey.SHIP_SHOUTMESSAGE_SECONDS_REMAINING, shipLeft.Seconds.ToString()), MsgMessageType.Middle);
        }
    }
}