using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Game.Extensions.RelationsExtensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.Enums.Relations;

namespace NosEmu.Plugins.BasicImplementations.Event.Relations;

public class RelationBlockEventHandler : IAsyncEventProcessor<RelationBlockEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly SerializableGameServer _serializableGameServer;
    private readonly ISessionManager _sessionManager;

    public RelationBlockEventHandler(IGameLanguageService gameLanguage, ISessionManager sessionManager, SerializableGameServer serializableGameServer)
    {
        _gameLanguage = gameLanguage;
        _sessionManager = sessionManager;
        _serializableGameServer = serializableGameServer;
    }

    public async Task HandleAsync(RelationBlockEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (session.CurrentMapInstance == null)
        {
            return;
        }

        if (session.PlayerEntity.Id == e.CharacterId)
        {
            return;
        }

        IClientSession target = _sessionManager.GetSessionByCharacterId(e.CharacterId);
        if (target == null)
        {
            return;
        }

        if (session.PlayerEntity.IsMarried(e.CharacterId))
        {
            return;
        }

        if (session.PlayerEntity.IsInFamily() && session.PlayerEntity.GetFamilyMembers().FirstOrDefault(x => x.CharacterId == e.CharacterId) != null)
        {
            return;
        }

        if (session.PlayerEntity.IsFriend(e.CharacterId))
        {
            return;
        }

        if (session.PlayerEntity.IsBlocking(e.CharacterId))
        {
            return;
        }

        if (_serializableGameServer.ChannelType == GameChannelType.ACT_4)
        {
            if (target.PlayerEntity.Faction != session.PlayerEntity.Faction)
            {
                return;
            }
        }

        await session.AddRelationAsync(e.CharacterId, CharacterRelationType.Blocked);
        string targetName = target.PlayerEntity.Name;
        session.SendInfo(_gameLanguage.GetLanguageFormat(GameDialogKey.BLACKLIST_INFO_ADDED, session.UserLanguage, targetName ?? "?"));
    }
}