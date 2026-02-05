using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinilandSignPostJoinEventHandler : IAsyncEventProcessor<MinilandSignPostJoinEvent>
{
    private readonly IMinilandManager _minilandManager;

    public MinilandSignPostJoinEventHandler(IMinilandManager minilandManager) => _minilandManager = minilandManager;

    public async Task HandleAsync(MinilandSignPostJoinEvent e, CancellationToken cancellation)
    {
        long minilandPlayerId = e.PlayerId;

        IClientSession session = e.Sender;
        if (minilandPlayerId == session.PlayerEntity.Id)
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.HAS_SIGNPOSTS_ENABLED))
        {
            return;
        }

        INpcEntity findSignPost = session.CurrentMapInstance.GetPassiveNpcs().FirstOrDefault(x => x.MinilandOwner != null && x.MinilandOwner.Id == minilandPlayerId);
        IPlayerEntity minilandOwner = findSignPost?.MinilandOwner;
        if (minilandOwner == null)
        {
            return;
        }

        // Don't use minilandOwner.Miniland :peepoGun:
        IMapInstance miniland = _minilandManager.GetMinilandByCharacterId(minilandOwner.Id);
        if (miniland == null)
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.INFORMATION_INFO_PLAYER_OFFLINE));
            return;
        }

        if (minilandOwner.MinilandState == MinilandState.LOCK || minilandOwner.MinilandState == MinilandState.PRIVATE
            && !session.PlayerEntity.IsFriend(minilandOwner.Id) && !session.PlayerEntity.IsMarried(minilandOwner.Id) && !session.IsGameMaster())
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_CLOSED), MsgMessageType.Middle);
            return;
        }

        int count = miniland.Sessions.Count(x => x.PlayerEntity.Id != minilandOwner.Id && !x.GmMode);
        int capacity = _minilandManager.GetMinilandMaximumCapacity(minilandOwner.Id);

        if (count > capacity)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_FULL), MsgMessageType.Middle);
            return;
        }

        session.ChangeMap(miniland);
    }
}