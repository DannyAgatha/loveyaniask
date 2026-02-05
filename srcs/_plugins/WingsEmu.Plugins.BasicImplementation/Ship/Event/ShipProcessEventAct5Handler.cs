using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Ship;
using WingsEmu.Game.Ship.Configuration;
using WingsEmu.Game.Ship.Event;

namespace NosEmu.Plugins.BasicImplementations.Ship.Event;

public class ShipProcessEventAct5Handler : IAsyncEventProcessor<ShipProcessEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IRandomGenerator _randomGenerator;

    public ShipProcessEventAct5Handler(IRandomGenerator randomGenerator, IGameLanguageService languageService)
    {
        _randomGenerator = randomGenerator;
        _languageService = languageService;
    }

    public async Task HandleAsync(ShipProcessEvent e, CancellationToken cancellation)
    {
        if (e.ShipInstance.ShipType != ShipType.Act5)
        {
            return;
        }

        ProcessDeparture(e.ShipInstance, e.CurrentTime);
    }

    private void ProcessDeparture(ShipInstance shipInstance, DateTime currentTime)
    {
        if (currentTime < shipInstance.LastDeparture + shipInstance.Configuration.Departure)
        {
            return;
        }

        foreach (IClientSession session in shipInstance.MapInstance.Sessions.ToArray())
        {
            if (!session.PlayerEntity.RemoveGold(shipInstance.Configuration.ShipCost))
            {
                session.SendErrorChatMessage(_languageService.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage));
                session.ChangeToLastBaseMap();
                continue;
            }
            
            session.EmitEvent(new IncreaseBattlePassObjectiveEvent(MissionType.SpendXGoldToNpc, shipInstance.Configuration.ShipCost));
            session.ChangeMap(shipInstance.Configuration.DestinationMapId,
                (short)_randomGenerator.RandomNumber(shipInstance.Configuration.DestinationMapX.Minimum, shipInstance.Configuration.DestinationMapX.Maximum + 1),
                (short)_randomGenerator.RandomNumber(shipInstance.Configuration.DestinationMapY.Minimum, shipInstance.Configuration.DestinationMapY.Maximum + 1));
        }

        shipInstance.DepartureWarnings = shipInstance.Configuration.DepartureWarnings.ToList();
        shipInstance.LastDeparture = currentTime;
    }
}