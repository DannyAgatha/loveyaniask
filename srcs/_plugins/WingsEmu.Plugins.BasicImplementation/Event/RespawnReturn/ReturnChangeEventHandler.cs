using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Respawns;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RespawnReturn.Event;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.RespawnReturn;

public class ReturnChangeEventHandler : IAsyncEventProcessor<ReturnChangeEvent>
{
    private readonly IGameLanguageService _gameLanguage;

    public ReturnChangeEventHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public async Task HandleAsync(ReturnChangeEvent e, CancellationToken cancellation)
    {
        int mapId = e.MapId;
        short mapX = e.MapX;
        short mapY = e.MapY;
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && !e.IsByGroup)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        session.PlayerEntity.HomeComponent.ChangeReturn(new CharacterReturnDto
        {
            MapId = (short)mapId,
            MapX = mapX,
            MapY = mapY
        });
    }
}