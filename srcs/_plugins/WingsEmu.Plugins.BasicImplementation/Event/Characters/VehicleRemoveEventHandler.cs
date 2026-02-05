using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.Vehicles;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class VehicleRemoveEventHandler : IAsyncEventProcessor<RemoveVehicleEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartner;
    private readonly IVehicleConfigurationProvider _provider;

    public VehicleRemoveEventHandler(IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartner, IVehicleConfigurationProvider provider)
    {
        _gameLanguage = gameLanguage;
        _spPartner = spPartner;
        _provider = provider;
    }

    public async Task HandleAsync(RemoveVehicleEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        if (e.ShowMates && !session.PlayerEntity.IsInvisible())
        {
            session.BroadcastInTeamMembers(_gameLanguage, _spPartner);
        }

        session.PlayerEntity.RandomMapTeleport = null;
        session.RefreshParty(_spPartner);

        Buff speedBooster = session.PlayerEntity.BuffComponent.GetBuff((short)BuffVnums.SPEED_BOOSTER);
        await session.PlayerEntity.RemoveBuffAsync(false, speedBooster);

        VehicleConfiguration vehicle = _provider.GetByMorph(session.PlayerEntity.Morph, session.PlayerEntity.Gender);
        
        if (vehicle?.VehicleBuffs != null)
        {
            var vehicleBuffIds = vehicle.VehicleBuffs.Select(b => b.BuffId).ToList();

            foreach (Buff activeBuff in session.PlayerEntity.BuffComponent.GetAllBuffs())
            {
                if (!vehicleBuffIds.Contains(activeBuff.CardId))
                {
                    continue;
                }

                Buff buffToRemove = session.PlayerEntity.BuffComponent.GetBuff(activeBuff.CardId);
                if (buffToRemove != null)
                {
                    await session.PlayerEntity.RemoveBuffAsync(true, buffToRemove);
                }
            }
        }

        if (vehicle?.VehicleBoostType != null)
        {
            foreach (VehicleBoost vehicleBoost in vehicle.VehicleBoostType)
            {
                switch (vehicleBoost.BoostType)
                {
                    case BoostType.CREATE_BUFF:

                        if (vehicleBoost.SecondValue.HasValue)
                        {
                            await session.PlayerEntity.RemoveBuffAsync((int)vehicleBoost.SecondValue);
                        }

                        break;

                    case BoostType.CREATE_BUFF_ON_END:

                        if (vehicleBoost.FirstValue.HasValue)
                        {
                            await session.PlayerEntity.RemoveBuffAsync((int)vehicleBoost.FirstValue);
                        }

                        break;
                }
            }
        }

        session.PlayerEntity.IsOnVehicle = false;
        await session.EmitEventAsync(new GetDefaultMorphEvent());
        session.RefreshStatChar();
        session.BroadcastEq();
        session.RefreshStat();
        session.SendCondPacket();
        session.PlayerEntity.LastSpeedChange = DateTime.UtcNow;
    }
}