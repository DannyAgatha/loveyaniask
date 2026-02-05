using System;
using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class ChangeSpecialistSlotGuriHandler : IGuriHandler
{
    private readonly IDelayManager _delayManager;
    private readonly ICharacterAlgorithm _characterAlgorithm;

    public ChangeSpecialistSlotGuriHandler(IDelayManager delayManager, ICharacterAlgorithm characterAlgorithm)
    {
        _delayManager = delayManager;
        _characterAlgorithm = characterAlgorithm;
    }

    public long GuriEffectId => 99999;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        if (!await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.ChangeSpecialistSlot))
        {
            return;
        }

        switch (e.Data)
        {
            case 1 when !session.PlayerEntity.Specialist.IsSecondSpecialistSlotActivated:
            case 2 when !session.PlayerEntity.Specialist.IsThirdSpecialistSlotActivated:
                return;
        }

        session.PlayerEntity.Specialist.CurrentActiveSpecialistSlot = (byte)e.Data;

        session.PlayerEntity.SpecialistComponent.RefreshSlStats(session.PlayerEntity.Specialist.CurrentActiveSpecialistSlot);
        session.RefreshStatChar();
        session.RefreshStat();

        session.SendSpecialistCardInfo(session.PlayerEntity.Specialist, _characterAlgorithm);
        session.PlayerEntity.LastSlotChange = DateTime.Now;
        session.SendDancePacket(false);
    }
}