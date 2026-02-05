using PhoenixLib.Events;
using Plugin.Alzanor.Managers;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorStartEventHandler: IAsyncEventProcessor<AlzanorStartEvent>
{
    private readonly AlzanorConfiguration _alzanorConfiguration;
    private readonly IAlzanorManager _alzanorManager;
    private readonly IAlzanorFactory _alzanorFactory;
    private readonly IRandomGenerator _randomGenerator;

    public AlzanorStartEventHandler(AlzanorConfiguration alzanorConfiguration, IAlzanorManager alzanorManager, IAlzanorFactory alzanorFactory, IRandomGenerator randomGenerator)
    {
        _alzanorConfiguration = alzanorConfiguration;
        _alzanorManager = alzanorManager;
        _alzanorFactory = alzanorFactory;
        _randomGenerator = randomGenerator;
    }

    public async Task HandleAsync(AlzanorStartEvent e, CancellationToken cancellation)
    {
        AlzanorParty alzanorParty = await _alzanorFactory.CreateAlzanorEvent(e.RedTeam, e.BlueTeam);
        if (alzanorParty == null)
        {
            return;
        }

        if (!_alzanorManager.IsActive)
        {
            _alzanorManager.IsActive = true;
        }
        _alzanorManager.AddAlzanor(alzanorParty);
        
        await HandleStart(alzanorParty, AlzanorTeamType.Red);
        await HandleStart(alzanorParty, AlzanorTeamType.Blue);
    }

    private async Task HandleStart(AlzanorParty alzanorParty, AlzanorTeamType teamType)
    {
        IReadOnlyList<IClientSession> members = teamType == AlzanorTeamType.Red ? alzanorParty.RedTeam : alzanorParty.BlueTeam;
        IMapInstance mapInstance = alzanorParty.MapInstance;
        GameDialogKey gameDialogKey = teamType == AlzanorTeamType.Red ? GameDialogKey.ALZANOR_SHOUTMESSAGE_RED_TEAM : GameDialogKey.ALZANOR_SHOUTMESSAGE_BLUE_TEAM;
        foreach (IClientSession member in members)
        {
            member.PlayerEntity.AlzanorComponent.SetAlzanorEvent(alzanorParty, teamType);
            member.PlayerEntity.Hp = member.PlayerEntity.MaxHp;
            member.PlayerEntity.Mp = member.PlayerEntity.MaxMp;
            
            await member.EmitEventAsync(new RemoveVehicleEvent());
            
            short randomX;
            short randomY;
            switch (teamType)
            {
                case AlzanorTeamType.Red:
                    randomX = (short)_randomGenerator.RandomNumber(_alzanorConfiguration.RedStartX, _alzanorConfiguration.RedEndX + 1);
                    randomY = (short)_randomGenerator.RandomNumber(_alzanorConfiguration.RedStartY, _alzanorConfiguration.RedEndY + 1);
                    if (mapInstance.IsBlockedZone(randomX, randomY))
                    {
                        randomX = 104;
                        randomY = 33;
                    }

                    break;
                case AlzanorTeamType.Blue:
                    randomX = (short)_randomGenerator.RandomNumber(_alzanorConfiguration.BlueStartX, _alzanorConfiguration.BlueEndX + 1);
                    randomY = (short)_randomGenerator.RandomNumber(_alzanorConfiguration.BlueStartY, _alzanorConfiguration.BlueEndY + 1);
                    if (mapInstance.IsBlockedZone(randomX, randomY))
                    {
                        randomX = 110;
                        randomY = 106;
                    }

                    break;
                default:
                    member.PlayerEntity.RainbowBattleComponent.RemoveRainbowBattle();
                    continue;
            }
            
            member.ChangeMap(mapInstance, randomX, randomY);
            Console.WriteLine($"Team {member.PlayerEntity.Name}: {member.PlayerEntity.AlzanorComponent.Team.ToString()}");
            
            _alzanorManager.RefreshAlzanorInstance();
            member.SendMsg(member.GetLanguage(gameDialogKey), MsgMessageType.Middle);
        }
    }
}