using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.Vehicles;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class SpeedBoosterEventHandler : IAsyncEventProcessor<SpeedBoosterEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly IMapManager _mapManager;
    private readonly IVehicleConfigurationProvider _provider;
    private readonly IRandomGenerator _randomGenerator;

    public SpeedBoosterEventHandler(IBuffFactory buffFactory, IMapManager mapManager, IVehicleConfigurationProvider provider, IRandomGenerator randomGenerator)
    {
        _buffFactory = buffFactory;
        _mapManager = mapManager;
        _provider = provider;
        _randomGenerator = randomGenerator;
    }

    public async Task HandleAsync(SpeedBoosterEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        if (session.PlayerEntity.HasBuff(BuffVnums.SPEED_BOOSTER))
        {
            return;
        }

        VehicleConfiguration vehicle = _provider.GetByMorph(session.PlayerEntity.Morph, session.PlayerEntity.Gender);

        if (vehicle?.VehicleBoostType == null)
        {
            return;
        }

        await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.SPEED_BOOSTER, session.PlayerEntity, TimeSpan.FromSeconds(BuffVehicleDuration(session.PlayerEntity))));
        session.BroadcastEffectInRange(EffectType.SpeedBoost);

        foreach (VehicleBoost vehicleBoost in vehicle.VehicleBoostType)
        {
            switch (vehicleBoost.BoostType)
            {
                case BoostType.REMOVE_BAD_EFFECTS:
                    await session.PlayerEntity.RemoveNegativeBuffs(vehicleBoost.FirstValue ?? 0);
                    break;
                case BoostType.REGENERATE_HP_MP:
                    if (!session.PlayerEntity.IsAlive())
                    {
                        break;
                    }

                    if (!vehicleBoost.FirstValue.HasValue)
                    {
                        break;
                    }

                    int toIncrease = session.PlayerEntity.Level * vehicleBoost.FirstValue.Value;

                    session.PlayerEntity.Hp += toIncrease;
                    session.PlayerEntity.BroadcastHeal(toIncrease);
                    if (session.PlayerEntity.Hp > session.PlayerEntity.MaxHp)
                    {
                        session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
                    }

                    session.PlayerEntity.Mp += toIncrease;
                    if (session.PlayerEntity.Mp > session.PlayerEntity.MaxMp)
                    {
                        session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
                    }

                    session.RefreshStat();
                    break;
                case BoostType.REGENERATE_MP:
                    if (!session.PlayerEntity.IsAlive())
                    {
                        break;
                    }

                    if (!vehicleBoost.FirstValue.HasValue)
                    {
                        break;
                    }

                    int toIncreasesmp = session.PlayerEntity.Level * vehicleBoost.FirstValue.Value;

                    session.PlayerEntity.BroadcastHeal(toIncreasesmp);

                    session.PlayerEntity.Mp += toIncreasesmp;
                    if (session.PlayerEntity.Mp > session.PlayerEntity.MaxMp)
                    {
                        session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
                    }

                    session.RefreshStat();
                    break;

                case BoostType.REGENERATE_HP:
                    if (!session.PlayerEntity.IsAlive())
                    {
                        break;
                    }

                    if (!vehicleBoost.FirstValue.HasValue)
                    {
                        break;
                    }

                    int toIncreaseshp = session.PlayerEntity.Level * vehicleBoost.FirstValue.Value;

                    session.PlayerEntity.Hp += toIncreaseshp;
                    session.PlayerEntity.BroadcastHeal(toIncreaseshp);
                    if (session.PlayerEntity.Hp > session.PlayerEntity.MaxHp)
                    {
                        session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
                    }

                    session.RefreshStat();
                    break;

                case BoostType.TELEPORT_FORWARD:
                    session.SendGuriPacket(1, 5);
                    break;
                case BoostType.RANDOM_TELEPORT_ON_MAP:
                    session.PlayerEntity.RandomMapTeleport = DateTime.UtcNow;
                    break;
                case BoostType.CREATE_BUFF:
                    {
                        if (!vehicleBoost.FirstValue.HasValue)
                        {
                            break;
                        }

                        if (!vehicleBoost.SecondValue.HasValue)
                        {
                            break;
                        }

                        short chance = vehicleBoost.FirstValue.Value;
                        short buffId = vehicleBoost.SecondValue.Value;

                        if (_randomGenerator.RandomNumber() > chance)
                        {
                            break;
                        }

                        if (buffId == (short)BuffVnums.DAZZLE)
                        {
                            IEnumerable<IBattleEntity> enemiesInRange = session.PlayerEntity.GetEnemiesInRange(session.PlayerEntity, 3);
                            foreach (IBattleEntity entity in enemiesInRange)
                            {
                                if (!entity.IsAlive())
                                {
                                    continue;
                                }

                                await entity.AddBuffAsync(_buffFactory.CreateBuff(buffId, session.PlayerEntity));
                            }
                        }
                        else
                        {
                            await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff(buffId, session.PlayerEntity));
                        }
                    }

                    break;
                case BoostType.DODGE_ALL_ATTACK:
                    {
                        if (!session.PlayerEntity.IsAlive())
                        {
                            break;
                        }

                        if (!vehicleBoost.FirstValue.HasValue)
                        {
                            break;
                        }

                        short chance = vehicleBoost.FirstValue.Value;
                        session.PlayerEntity.NoDamageChance = (sbyte)chance;

                        break;
                    }
            }
        }
    }

    private int BuffVehicleDuration(IPlayerEntity character)
    {
        VehicleConfiguration vehicle = _provider.GetByMorph(character.Morph, character.Gender);
        VehicleBoost boost = vehicle?.VehicleBoostType.FirstOrDefault(x => x.BoostType == BoostType.INCREASE_SPEED);

        if (boost?.FirstValue == null || !boost.SecondValue.HasValue)
        {
            return 3;
        }

        short speed = boost.FirstValue.Value;
        short duration = boost.SecondValue.Value;

        character.VehicleSpeed += (byte)speed;
        return duration;
    }
}