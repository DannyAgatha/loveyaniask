using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorLeaveEventHandler : IAsyncEventProcessor<AlzanorLeaveEvent>
{
    private readonly IAsyncEventPipeline _eventPipeline;

    public AlzanorLeaveEventHandler(IAsyncEventPipeline eventPipeline)
    {
        _eventPipeline = eventPipeline;
    }

    public async Task HandleAsync(AlzanorLeaveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        AlzanorParty alzanorParty = session.PlayerEntity.AlzanorComponent.AlzanorParty;
        if (alzanorParty == null)
        {
            return;
        }
        if (e.CheckIfFinished && alzanorParty.FinishTime == null)
        {
            return;
        }
        AlzanorTeamType team = session.PlayerEntity.AlzanorComponent.Team;
        IReadOnlyList<IClientSession> members = team == AlzanorTeamType.Red ? alzanorParty.RedTeam : alzanorParty.BlueTeam;
        switch (team)
        {
            case AlzanorTeamType.Red:
                session.PlayerEntity.AlzanorComponent.AlzanorParty.RemoveRedPlayer(session);
                break;
            case AlzanorTeamType.Blue:
                session.PlayerEntity.AlzanorComponent.AlzanorParty.RemoveBluePlayer(session);
                break;
        }
        
        foreach (IClientSession member in members)
        {
            if (e.SendMessage)
            {
                member.SendMsg(member.GetLanguageFormat(GameDialogKey.ALZANOR_MESSAGE_PLAYER_LEFT, session.PlayerEntity.Name), MsgMessageType.Middle);
                member.SendChatMessage(member.GetLanguageFormat(GameDialogKey.ALZANOR_MESSAGE_PLAYER_LEFT, session.PlayerEntity.Name), ChatMessageColorType.Yellow);
            }
        }
        
        await session.PlayerEntity.RemoveAllBuffsAsync(false);
        
        session.PlayerEntity.AlzanorComponent.RemoveAlzanorEvent();
        session.ChangeToLastBaseMap();
    }

}