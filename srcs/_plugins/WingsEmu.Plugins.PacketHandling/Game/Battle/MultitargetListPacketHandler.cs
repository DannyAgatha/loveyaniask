// NosEmu
// 


using System;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Battle;

public class MultiTargetListPacketHandler : GenericGamePacketHandlerBase<MultiTargetListPacket>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISkillUsageManager _skillUsageManager;
    
    public MultiTargetListPacketHandler(IGameLanguageService gameLanguage, ISkillUsageManager skillUsageManager)
    {
        _gameLanguage = gameLanguage;
        _skillUsageManager = skillUsageManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, MultiTargetListPacket packet)
    {
        if (session.IsMuted())
        {
            session.SendMuteMessage();
            return;
        }

        if ((DateTime.UtcNow - session.PlayerEntity.LastTransform).TotalSeconds < 3)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_CANT_ATTACK_YET, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (packet.TargetsAmount <= 0 || packet.TargetsAmount != packet.Targets.Count || packet.Targets == null)
        {
            return;
        }

        _skillUsageManager.SetMultiTargets(session.PlayerEntity.Id, packet.Targets.Select(x => (x.TargetType, (long)x.TargetId)).ToList());
    }
}