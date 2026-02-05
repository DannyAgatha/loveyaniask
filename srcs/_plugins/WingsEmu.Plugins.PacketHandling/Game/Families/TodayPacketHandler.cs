using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class TodayPacketHandler : GenericGamePacketHandlerBase<TodayPacket>
{
    private readonly IExpirableLockService _lock;

    public TodayPacketHandler(IExpirableLockService @lock) => _lock = @lock;

    protected override async Task HandlePacketAsync(IClientSession session, TodayPacket packet)
    {
        if (!session.PlayerEntity.IsInFamily())
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY));
            return;
        }

        if (session.PlayerEntity.Level < 30)
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.FAMILY_INFO_TODAY_LOW_LEVEL));
            return;
        }

        if (await _lock.ExistsTemporaryLock($"game:locks:family:{session.PlayerEntity.Family.Id}:{session.PlayerEntity.Id}:quote-of-the-day"))
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.FAMILY_INFO_USED_DAILY_MESSAGE));
            return;
        }

        session.SendPacket("today_stc");
    }
}