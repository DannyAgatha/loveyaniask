using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalStartProcedureEventArenaHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly PlayerRevivalConfiguration _revivalConfiguration;

    public RevivalStartProcedureEventArenaHandler(GameRevivalConfiguration gameRevivalConfiguration, IGameLanguageService gameLanguage)
    {
        _gameLanguage = gameLanguage;
        _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;
    }

    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.MapInstance.MapInstanceType != MapInstanceType.ArenaInstance)
        {
            return;
        }

        DateTime actualTime = DateTime.UtcNow;

        if (e.Sender.PlayerEntity.IsOnVehicle)
        {
            await e.Sender.EmitEventAsync(new RemoveVehicleEvent());
        }

        IBattleEntity killer = e.Killer;
        if (killer != null)
        {
            IPlayerEntity playerKiller = killer switch
            {
                IPlayerEntity playerEntity => playerEntity,
                IMateEntity mateEntity => mateEntity.Owner,
                IMonsterEntity monsterEntity => monsterEntity.SummonerType != null && monsterEntity.SummonerId != null && monsterEntity.SummonerType == VisualType.Player
                    ? monsterEntity.MapInstance.GetCharacterById(monsterEntity.SummonerId.Value)
                    : null,
                _ => null
            };

            if (playerKiller != null)
            {
                e.Sender.CurrentMapInstance.Broadcast(x =>
                {
                    return x.GenerateMsgPacket(_gameLanguage.GetLanguageFormat(GameDialogKey.ARENA_SHOUTMESSAGE_PVP_KILL,
                        x.UserLanguage, e.Sender.PlayerEntity.Name, playerKiller.Name), MsgMessageType.Middle);
                });

                e.Sender.CurrentMapInstance.Broadcast(x =>
                {
                    return x.GenerateSayPacket(_gameLanguage.GetLanguageFormat(GameDialogKey.ARENA_SHOUTMESSAGE_PVP_KILL,
                        x.UserLanguage, e.Sender.PlayerEntity.Name, playerKiller.Name), ChatMessageColorType.Yellow);
                });

                e.Sender.PlayerEntity.LifetimeStats.TotalArenaDeaths++;
                playerKiller.LifetimeStats.TotalArenaKills++;

                if (e.Sender.PlayerEntity.IsInGroup())
                {
                    e.Sender.PlayerEntity.GetGroup().ArenaDeaths++;
                    foreach (IPlayerEntity member in e.Sender.PlayerEntity.GetGroup().Members)
                    {
                        if (member?.MapInstance is not { MapInstanceType: MapInstanceType.ArenaInstance })
                        {
                            continue;
                        }

                        member.Session.SendArenaStatistics(false);
                    }
                }
                else
                {
                    e.Sender.SendArenaStatistics(false);
                }

                if (playerKiller.IsInGroup())
                {
                    playerKiller.GetGroup().ArenaKills++;
                    foreach (IPlayerEntity member in playerKiller.GetGroup().Members)
                    {
                        if (member?.MapInstance is not { MapInstanceType: MapInstanceType.ArenaInstance })
                        {
                            continue;
                        }

                        member.Session.SendArenaStatistics(false);
                    }
                }
                else
                {
                    playerKiller.Session.SendArenaStatistics(false);
                }
            }
        }

        await e.Sender.PlayerEntity.RemoveBuffsOnDeathAsync();
        e.Sender.RefreshStat();

        e.Sender.PlayerEntity.UpdateAskRevival(actualTime + _revivalConfiguration.RevivalDialogDelay, AskRevivalType.ArenaRevival);
        e.Sender.PlayerEntity.UpdateRevival(actualTime + _revivalConfiguration.ForcedRevivalDelay, RevivalType.TryPayRevival, ForcedType.Forced);
    }
}