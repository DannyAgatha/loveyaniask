using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using Polly;
using Polly.Retry;
using WingsAPI.Communication;
using WingsAPI.Communication.Mail;
using WingsEmu.DTOs.Mails;

namespace NosEmu.Plugins.BasicImplementations.Mail;

public class MailCreationManager : BackgroundService
{
    private static readonly TimeSpan RefreshDelay = TimeSpan.FromSeconds(Convert.ToInt32(Environment.GetEnvironmentVariable("MAIL_MANAGER_REFRESH_DELAY") ?? "5"));

    private readonly ConcurrentQueue<CharacterMailDto> _mailQueue = new();

    private readonly IMailService _mailService;

    public MailCreationManager(IMailService mailService) => _mailService = mailService;

    public void AddCreateMailRequest(CreateMailRequest request)
    {
        _mailQueue.Enqueue(new CharacterMailDto
        {
            Date = DateTime.UtcNow,
            SenderName = request.SenderName,
            ReceiverId = request.ReceiverId,
            MailGiftType = request.MailGiftType,
            ItemInstance = request.ItemInstance
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMailsRequestsAsync();
            }
            catch (Exception e)
            {
                Log.Error("[MAIL_MANAGER]", e);
            }

            await Task.Delay(RefreshDelay, stoppingToken);
        }
    }

    private async Task ProcessPendingMailsRequestsAsync()
    {
        if (_mailQueue.IsEmpty)
        {
            return;
        }

        List<CharacterMailDto> characterMailDtos = new();

        while (_mailQueue.TryDequeue(out CharacterMailDto mailDto))
        {
            characterMailDtos.Add(mailDto);
        }

        List<CharacterMailDto> unsavedMails = new();
        AsyncRetryPolicy policy = Policy.Handle<Exception>().RetryAsync(3, (exception, i1) => Log.Error($"[MAIL_MANAGER] Failed to handle the mails, try {i1.ToString()}. ", exception));
        foreach (List<CharacterMailDto> dtos in SplitList(characterMailDtos, 100))
        {
            CreateMailBatchResponse response = null;
            try
            {
                response = await policy.ExecuteAsync(() => _mailService.CreateMailBatchAsync(new CreateMailBatchRequest
                {
                    Mails = dtos,
                    Bufferized = true
                }));
            }
            catch (Exception e)
            {
                Log.Error("[MAIL_MANAGER]", e);
            }

            if (response?.Status != RpcResponseType.SUCCESS)
            {
                unsavedMails.AddRange(dtos);
            }
        }

        foreach (CharacterMailDto unsavedMail in unsavedMails)
        {
            _mailQueue.Enqueue(unsavedMail);
        }
    }

    private static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
    {
        for (int i = 0; i < locations.Count; i += nSize)
        {
            yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
        }
    }
}