using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Relations;
using WingsEmu.Game.Skills;
using WingsEmu.Game.Warehouse;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class CharacterRemoveManagersEventHandler : IAsyncEventProcessor<CharacterRemoveManagersEvent>
{
    private readonly IAccountWarehouseManager _accountWarehouseManager;
    private readonly IInvitationManager _invitationManager;
    private readonly IMeditationManager _meditationManager;
    private readonly ISacrificeManager _sacrificeManager;
    private readonly ISpyOutManager _spyOutManager;
    private readonly ITeleportManager _teleportManager;

    public CharacterRemoveManagersEventHandler(IMeditationManager meditationManager, ITeleportManager teleportManager, ISpyOutManager spyOutManager,
        ISacrificeManager sacrificeManager, IInvitationManager invitationManager, IAccountWarehouseManager accountWarehouseManager)
    {
        _meditationManager = meditationManager;
        _teleportManager = teleportManager;
        _spyOutManager = spyOutManager;
        _sacrificeManager = sacrificeManager;
        _invitationManager = invitationManager;
        _accountWarehouseManager = accountWarehouseManager;
    }

    public async Task HandleAsync(CharacterRemoveManagersEvent e, CancellationToken cancellation)
    {
        long id = e.Sender.PlayerEntity.Id;
        long accountId = e.Sender.Account.Id;
        IPlayerEntity character = e.Sender.PlayerEntity;

        _meditationManager.RemoveAllMeditation(character);
        _teleportManager.RemovePosition(id);
        _spyOutManager.RemoveSpyOutSkill(id);

        IBattleEntity target = _sacrificeManager.GetTarget(character);
        if (target != null)
        {
            IBattleEntity caster = _sacrificeManager.GetCaster(target);
            _sacrificeManager.RemoveSacrifice(character, target);
            if (caster != null)
            {
                _sacrificeManager.RemoveSacrifice(caster, character);
            }
        }

        _invitationManager.RemoveAllPendingInvitations(id);
        _accountWarehouseManager.CleanCache(accountId);
    }
}