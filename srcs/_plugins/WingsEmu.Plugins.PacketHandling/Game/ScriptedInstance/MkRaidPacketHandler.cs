using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.ScriptedInstance;

public class MkRaidPacketHandler : GenericGamePacketHandlerBase<MkraidPacket>
{
    private readonly IGameLanguageService _gameLanguage;

    public MkRaidPacketHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    protected override async Task HandlePacketAsync(IClientSession session, MkraidPacket packet)
    {
        if (!session.PlayerEntity.IsInRaidParty)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.RAID_CHATMESSAGE_START_NOT_IN_RAID_PARTY, session.UserLanguage), ChatMessageColorType.PlayerSay);
            return;
        }

        if (!session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.RAID_CHATMESSAGE_START_IS_NOT_LEADER, session.UserLanguage), ChatMessageColorType.PlayerSay);
            return;
        }

        if (session.PlayerEntity.Raid.Started)
        {
            return;
        }

        IPortalEntity portal = session.CurrentMapInstance.Portals.FirstOrDefault(s =>
            session.PlayerEntity.PositionY >= s.PositionY - 1 &&
            session.PlayerEntity.PositionY <= s.PositionY + 1 &&
            session.PlayerEntity.PositionX >= s.PositionX - 1 &&
            session.PlayerEntity.PositionX <= s.PositionX + 1);

        if (portal == null || portal.Type != PortalType.Raid || portal.RaidType == null)
        {
            return;
        }

        if (session.PlayerEntity.Raid.Type != (RaidType)portal.RaidType.Value)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.RAID_CHATMESSAGE_START_RAID_TYPE_IS_WRONG, session.UserLanguage), ChatMessageColorType.PlayerSay);
            return;
        }

        await session.EmitEventAsync(new RaidInstanceStartEvent());
    }
}