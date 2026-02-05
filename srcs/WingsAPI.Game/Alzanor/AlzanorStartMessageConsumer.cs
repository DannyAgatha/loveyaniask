using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Communication;

namespace WingsEmu.Game.Alzanor;

public class AlzanorStartMessageConsumer : IMessageConsumer<AlzanorStartMessage>
{
    private readonly IAlzanorManager _alzanorManager;

    public AlzanorStartMessageConsumer(IAlzanorManager alzanorManager)
    {
        _alzanorManager = alzanorManager;
    }

    public async Task HandleAsync(AlzanorStartMessage notification, CancellationToken token)
    {

        await DiscordWebhook.SendWebhookAsync("https://discord.com/api/webhooks/1348072817563930704/YkfUGkVVbGMJagl77OvE6M0qN4bWRgpq8dZUNPtgZ707NjSGYxllreKR6baRxD52Z_RH", "Alzanor Battle Starts.", "Alzanor Battle Started. @everyone", "https://nosapki.com/images/npcs/3132.png");

        _alzanorManager.AlzanorProcessTime = DateTime.UtcNow;
    }
}