using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.Revival;
using WingsEmu.Game.Skills;
using WingsEmu.Game.TimeSpaces.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class PlayerDeathEventHandler : IAsyncEventProcessor<PlayerDeathEvent>
{
    private readonly IMeditationManager _meditation;
    private readonly ISpyOutManager _spyOutManager;

    public PlayerDeathEventHandler(IMeditationManager meditation, ISpyOutManager spyOutManager)
    {
        _meditation = meditation;
        _spyOutManager = spyOutManager;
    }

    public async Task HandleAsync(PlayerDeathEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IPlayerEntity character = session.PlayerEntity;

        if (character == null || character.IsAlive())
        {
            return;
        }

        if (_spyOutManager.ContainsSpyOut(character.Id))
        {
            _spyOutManager.RemoveSpyOutSkill(character.Id);
            session.SendObArPacket();
        }

        await character.Session.EmitEventAsync(new RemoveVehicleEvent());
        await character.Session.EmitEventAsync(new GetDefaultMorphEvent());
        await character.Session.EmitEventAsync(new RemoveAdditionalHpMpEvent
        {
            Hp = character.AdditionalHp,
            Mp = character.AdditionalMp
        });

        character.ClearFoodBuffer();
        character.ClearSnackBuffer();
        character.BCardComponent.ClearChargeBCard();
        character.ChargeComponent.ResetCharge();
        character.HitsByMonsters.Clear();
        character.Killer = e.Killer;
        character.LastDeath = DateTime.UtcNow;
        character.RemoveMeditation(_meditation);

        switch (session.CurrentMapInstance.MapInstanceType)
        {
            case MapInstanceType.RaidInstance:
                await session.EmitEventAsync(new RaidDiedEvent());
                break;
            case MapInstanceType.TimeSpaceInstance:
                await session.EmitEventAsync(new TimeSpaceDeathEvent());
                break;
            case MapInstanceType.Alzanor:
                await session.EmitEventAsync(new AlzanorDeathEvent(character.Killer));
                break;
        }

        await session.EmitEventAsync(new RevivalStartProcedureEvent(e.Killer));
    }
}