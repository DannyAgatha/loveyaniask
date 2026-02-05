using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Ship.Configuration;
using WingsEmu.Game.Ship.Event;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Ship.Event;

public class ShipProcessEventHandler : IAsyncEventProcessor<ShipProcessEvent>
{
    private readonly IGameLanguageService _languageService;
    public ShipProcessEventHandler(IGameLanguageService languageService) => _languageService = languageService;

    public async Task HandleAsync(ShipProcessEvent e, CancellationToken cancellation)
    {
        if (e.ShipInstance.DepartureWarnings.Count < 1)
        {
            return;
        }

        TimeSpan currentWarning = e.ShipInstance.DepartureWarnings.First();

        if (e.CurrentTime < e.ShipInstance.LastDeparture + currentWarning)
        {
            return;
        }

        e.ShipInstance.DepartureWarnings.Remove(currentWarning);

        TimeSpan timeLeft = e.ShipInstance.Configuration.Departure - currentWarning;
        bool isSeconds = timeLeft.TotalMinutes < 1;
        byte timeLeftValue = (byte)(isSeconds ? timeLeft.Seconds : timeLeft.Minutes);

        if (e.ShipInstance.ShipType == ShipType.Act7)
        {
            e.ShipInstance.MapInstance.Broadcast(x =>
                x.GenerateMsgiPacket(
                    MessageType.Default,
                    Game18NConstString.TransportToMoritiusShortly,
                    4,
                    isSeconds ? timeLeft.Seconds : timeLeft.Minutes).ToString()
            );
        }
        else
        {
            GameDialogKey key = isSeconds ? GameDialogKey.SHIP_SHOUTMESSAGE_SECONDS_REMAINING : GameDialogKey.SHIP_SHOUTMESSAGE_MINUTES_REMAINING;
            e.ShipInstance.MapInstance.Broadcast(x =>
                x.GenerateMsgPacket(_languageService.GetLanguageFormat(key, x.UserLanguage, timeLeftValue.ToString()), MsgMessageType.Middle));
        }
    }
}