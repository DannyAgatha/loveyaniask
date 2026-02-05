using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Mail;
using WingsAPI.Communication.Player;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Mails;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.Essentials.Administrator;

[Name("MailModule")]
[Description("Module related to mail admin commands.")]
[RequireAuthority(AuthorityType.Owner)]
public class AdministratorMailModule : SaltyModuleBase
{
    private readonly ICharacterService _characterService;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IMailService _mailService;
    private readonly ISessionManager _sessionManager;

    public AdministratorMailModule(IGameItemInstanceFactory gameItemInstanceFactory, ISessionManager sessionManager,
        ICharacterService characterService, IMailService mailService)
    {
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _sessionManager = sessionManager;
        _characterService = characterService;
        _mailService = mailService;
    }

    [Command("parcel", "mail", "gift")]
    [Description("Send gift mail to someone")]
    public async Task<SaltyCommandResult> ParcelAsync(string targetName, int itemVnum, int amount)
    {
        IClientSession session = Context.Player;
        IClientSession target = _sessionManager.GetSessionByCharacterName(targetName);
        if (target == null)
        {
            return await ParcelOffAsync(targetName, itemVnum, amount);
        }

        GameItemInstance item = _gameItemInstanceFactory.CreateItem(itemVnum, amount);
        await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, target.PlayerEntity.Id, MailGiftType.Normal, item));
        return new SaltyCommandResult(true, "Parcel has been sent.");
    }

    private async Task<SaltyCommandResult> ParcelOffAsync(string targetName, int itemVnum, int amount)
    {
        ClusterCharacterInfo tmp = _sessionManager.GetOnlineCharacterByName(targetName);
        if (tmp != null)
        {
            return new SaltyCommandResult(false, $"Player found on channel: {tmp.ChannelId}");
        }

        DbServerGetCharacterResponse response = await _characterService.GetCharacterByName(new DbServerGetCharacterRequestByName
        {
            CharacterName = targetName
        });

        if (response?.RpcResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, "Player not found");
        }

        if (response.CharacterDto.Name != targetName)
        {
            return new SaltyCommandResult(false, "Player not found");
        }

        GameItemInstance item = _gameItemInstanceFactory.CreateItem(itemVnum, amount);
        ItemInstanceDTO itemInstanceDto = _gameItemInstanceFactory.CreateDto(item);

        await _mailService.CreateMailBatchAsync(new CreateMailBatchRequest
        {
            Mails = Lists.Create(new CharacterMailDto
            {
                Date = DateTime.UtcNow,
                SenderName = Context.Player.PlayerEntity.Name,
                ReceiverId = response.CharacterDto.Id,
                MailGiftType = MailGiftType.Normal,
                ItemInstance = itemInstanceDto
            }),
            Bufferized = true
        });

        return new SaltyCommandResult(true, "Parcel has been sent.");
    }

    [Command("parcel-all", "mail-all", "gift-all")]
    [Description("Send gift mail to someone")]
    public async Task<SaltyCommandResult> ParcelAsync(int itemVnum, int amount)
    {
        string senderName = Context.Player.PlayerEntity.Name;
        // todo - remove this shit :special:
        HashSet<string> ips = new();
        foreach (IClientSession session in _sessionManager.Sessions)
        {
            if (ips.Contains(session.IpAddress))
            {
                Context.Player.SendChatMessage($"[{session.IpAddress}] {session.PlayerEntity.Name} got already this gift.", ChatMessageColorType.Red);
                continue;
            }

            GameItemInstance item = _gameItemInstanceFactory.CreateItem(itemVnum, amount);
            await session.EmitEventAsync(new MailCreateEvent(senderName, session.PlayerEntity.Id, MailGiftType.Normal, item));
            ips.Add(session.IpAddress);
        }

        return new SaltyCommandResult(true, "Parcel has been sent.");
    }

    [Command("note")]
    [Description("Send note to someone")]
    public async Task<SaltyCommandResult> NoteAsync(IClientSession target, string title, [Remainder] string message)
    {
        IClientSession session = Context.Player;
        if (target == null)
        {
            return new SaltyCommandResult(false, "Player is offline.");
        }

        await session.EmitEventAsync(new NoteCreateEvent(target.PlayerEntity.Name, title, message));
        return new SaltyCommandResult(true, "Note has been sent.");
    }
}