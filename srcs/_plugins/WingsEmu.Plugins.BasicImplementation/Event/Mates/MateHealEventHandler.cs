using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateHealEventHandler : IAsyncEventProcessor<MateHealEvent>
{
    public async Task HandleAsync(MateHealEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IMateEntity mateEntity = e.MateEntity;

        if (!mateEntity.IsAlive())
        {
            return;
        }

        int mateMaxHp = mateEntity.MaxHp;
        int mateMaxMp = mateEntity.MaxMp;

        int mateHpHeal = mateEntity.Hp + e.HpHeal > mateMaxHp ? mateMaxHp - mateEntity.Hp : e.HpHeal;
        int mateMpHeal = mateEntity.Mp + e.MpHeal > mateMaxMp ? mateMaxMp - mateEntity.Mp : e.MpHeal;

        mateEntity.Hp += mateHpHeal;
        mateEntity.Mp += mateMpHeal;

        session.CurrentMapInstance?.Broadcast(mateEntity.GenerateRc(mateHpHeal));
        session.SendPacket(mateEntity.GenerateStatInfo());
    }
}