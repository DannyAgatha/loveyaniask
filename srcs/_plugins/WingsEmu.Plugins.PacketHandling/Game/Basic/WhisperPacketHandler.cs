using System.Collections.Concurrent;
using System;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Game._i18n;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Game.Extensions;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class WhisperPacketHandler : GenericGamePacketHandlerBase<WhisperPacket>
{
    private static readonly ConcurrentDictionary<long, DateTime> LastMessageTimestamps = new();
    private readonly IGameLanguageService _gameLanguageService;

    public WhisperPacketHandler(IGameLanguageService gameLanguageService)
    {
        _gameLanguageService = gameLanguageService;
    }

    protected override async Task HandlePacketAsync(IClientSession session, WhisperPacket whisperPacket)
    {
        if (string.IsNullOrEmpty(whisperPacket.Message) || whisperPacket.Message.Length < 2)
        {
            return;
        }
        
        string[] messageSplit = whisperPacket.Message.Split(' ');
        string characterName = messageSplit[0];
        string message = string.Join(" ", messageSplit.Skip(1));

        if (message.Length > 60)
        {
            message = message.Substring(0, 60);
        }

        message = message.Trim();

        await session.EmitEventAsync(new InterChannelSendWhisperEvent(characterName, message));
    }
}