using System;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.ScriptedInstance;

public class BscPacketHandler : GenericGamePacketHandlerBase<BscPacket>
{
    private readonly IGameLanguageService _language;
    private readonly IRainbowBattleManager _rainbowBattleManager;
    private readonly IServerManager _serverManager;

    public BscPacketHandler(IServerManager serverManager, IGameLanguageService language, IRainbowBattleManager rainbowBattleManager)
    {
        _serverManager = serverManager;
        _language = language;
        _rainbowBattleManager = rainbowBattleManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, BscPacket packet)
    {
        if (!Enum.TryParse(packet.Type.ToString(), out GameType type))
        {
            return;
        }

        switch (type)
        {
            case GameType.ArenaOfTalents:
                break;
            case GameType.RainbowBattle:
                if (!_rainbowBattleManager.RegisteredPlayers.Contains(session.PlayerEntity.Id))
                {
                    return;
                }

                _rainbowBattleManager.UnregisterPlayer(session.PlayerEntity.Id);
                session.SendBsInfoPacket(BsInfoType.CloseWindow, GameType.RainbowBattle, 0, QueueWindow.WaitForEntry);
                break;
        }
    }
}