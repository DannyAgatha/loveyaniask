// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Chat;

public class FamilyChatPacketHandler : GenericGamePacketHandlerBase<FamilyChatPacket>
{
    private readonly ISessionManager _sessionManager;

    public FamilyChatPacketHandler(ISessionManager sessionManager) => _sessionManager = sessionManager;

    protected override async Task HandlePacketAsync(IClientSession session, FamilyChatPacket packet)
    {
        if (string.IsNullOrEmpty(packet.Message))
        {
            return;
        }

        string message = packet.Message;

        if (message.Length > 60)
        {
            message = message.Substring(0, 60);
        }

        await session.EmitEventAsync(new FamilyChatMessageEvent(message));

        /*foreach (IClientSession targetSession in _sessionManager.Sessions.ToList())
        {
            if (!targetSession.HasSelectedCharacter)
            {
                return;
            }

            if (!session.Character.IsInFamily())
            {
                return;
            }

            if (!session.Character.IsInFamily())
            {
                return;
            }

            if (targetSession.Character.Family?.Id != session.Character.Family?.Id)
            {
                return;
            }

            if (session.HasCurrentMapInstance && targetSession.HasCurrentMapInstance && session.CurrentMapInstance == targetSession.CurrentMapInstance)
            {
                if (session.Account.Authority != AuthorityType.Moderator && !session.Character.CheatComponent.IsInvisible)
                {
                    targetSession.SendChatMessage(msg, ChatMessageColorType.Blue);
                }
                else
                {
                    targetSession.SendChatMessage(ccmsg, ChatMessageColorType.Blue);
                }
            }
            else
            {
                targetSession.SendChatMessage(ccmsg, ChatMessageColorType.Blue);
            }

            if (!session.Character.CheatComponent.IsInvisible)
            {
                targetSession.SendSpeak(msg, SpeakType.Family);
            }
        }*/
    }
}