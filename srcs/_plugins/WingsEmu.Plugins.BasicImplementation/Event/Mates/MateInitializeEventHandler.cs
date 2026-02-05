using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateInitializeEventHandler : IAsyncEventProcessor<MateInitializeEvent>
{
    private readonly IBattleEntityAlgorithmService _algorithm;

    public MateInitializeEventHandler(IBattleEntityAlgorithmService algorithm, INpcMonsterManager npcMonsterManager)
    {
        _algorithm = algorithm;
    }

    public async Task HandleAsync(MateInitializeEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IMateEntity mateEntity = e.MateEntity;

        mateEntity.Initialize();
        mateEntity.RefreshStatistics();

        if (e.IsOnCharacterEnter)
        {
            session.PlayerEntity.MateComponent.AddMate(mateEntity);
            return;
        }

        mateEntity.PetSlot = session.PlayerEntity.GetFreeMateSlot(mateEntity.MateType == MateType.Partner);
        session.PlayerEntity.MateComponent.AddMate(mateEntity);
        mateEntity.RefreshMaxHpMp(_algorithm);
        mateEntity.ChangePosition(new Position(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        session.SendPClearPacket();
    }
}