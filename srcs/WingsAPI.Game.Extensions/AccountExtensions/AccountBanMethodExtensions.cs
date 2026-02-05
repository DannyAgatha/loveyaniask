using System;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.AccountExtensions;

public static class AccountBanMethodExtensions
{
    public static async Task HandleInvalidBehavior(
        this IClientSession session,
        IAccountService accountService,
        string reason,
        Func<IClientSession, Task>? closeAction = null,
        Func<IClientSession, Task>? additionalAction = null
    )
    {
        await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.SEVERE_ABUSE, reason);
        
        if (closeAction is not null)
        {
            await closeAction(session);
        }
        
        if (additionalAction is not null)
        {
            await additionalAction(session);
        }
        
        var banRequest = new BanAccountRequest
        {
            AccountId = session.Account.Id,
            Reason = reason
        };

        AccountBanSaveResponse banResponse = await accountService.BanAccount(banRequest);
        if (banResponse.ResponseType == RpcResponseType.SUCCESS)
        {
            Log.Info($"Account ID [{session.Account.Id}] has been banned. Reason: {reason}");
            session.ForceDisconnect();
        }
    }
}