using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using ChatType = WingsEmu.Game._playerActionLogs.ChatType;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class InteractionGuriHandler : IGuriHandler
{
    private readonly IForbiddenNamesManager _forbiddenNamesManager;
    private readonly IItemsManager _itemsManager;
    private static readonly ConcurrentDictionary<long, DateTime> LastMessageTimestamps = new();

    private readonly IGameLanguageService _gameLanguageService;
    private readonly IGameLanguageService _languageService;
    private readonly ISpPartnerConfiguration _spPartner;

    public InteractionGuriHandler(IGameLanguageService languageService, IItemsManager itemsManager, ISpPartnerConfiguration spPartner,
        IForbiddenNamesManager forbiddenNamesManager, IGameLanguageService gameLanguageService)
    {
        _languageService = languageService;
        _itemsManager = itemsManager;
        _spPartner = spPartner;
        _forbiddenNamesManager = forbiddenNamesManager;
        _gameLanguageService = gameLanguageService;
    }

    public long GuriEffectId => 4;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (!long.TryParse(guriPacket.Packet[3], out long guriType))
        {
            return;
        }

        if (!long.TryParse(guriPacket.Packet[5], out long itemSpeaker))
        {
            return;
        }

        if (itemSpeaker == 999)
        {
            guriType = 999;
        }

        string message;
        string[] valueSplit;

        switch (guriType)
        {
            case 1:
                if (session.CantPerformActionOnAct4())
                {
                    return;
                }

                if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(guriPacket.Value))
                {
                    return;
                }

                if (!guriPacket.User.HasValue)
                {
                    return;
                }

                IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(s => s.Id == guriPacket.User && s.MateType == MateType.Pet);
                if (guriPacket.Value.Length > 15)
                {
                    session.SendInfo(_languageService.GetLanguage(GameDialogKey.ITEM_INFO_PET_NAME_TOO_LONG, session.UserLanguage));
                    return;
                }

                if (mateEntity == null)
                {
                    return;
                }

                var rg = new Regex(@"^[a-zA-Z0-9_\-\*]*$");
                if (rg.Matches(guriPacket.Value).Count != 1)
                {
                    return;
                }

                string expectedName = guriPacket.Value;
                mateEntity.MateName = expectedName;

                if (mateEntity.Position == default)
                {
                    mateEntity.ChangePosition(session.PlayerEntity.Position);
                }

                mateEntity.MapInstance.Broadcast(s => mateEntity.GenerateIn(_languageService, s.UserLanguage, _spPartner));

                session.RefreshParty(_spPartner);
                session.SendScnPackets();
                session.SendScpPackets();
                await session.RemoveItemFromInventory((short)ItemVnums.NAME_TAG);

                break;
            // Presentation message
            case 2:
                if (guriPacket.Value == null)
                {
                    return;
                }

                bool isLimitedItem = session.PlayerEntity.HasItem((short)ItemVnums.SELF_INTRODUCTION_LIMITED);
                if (!session.PlayerEntity.HasItem((short)ItemVnums.SELF_INTRODUCTION) && !isLimitedItem)
                {
                    return;
                }

                short itemVnumToRemove = isLimitedItem ? (short)ItemVnums.SELF_INTRODUCTION_LIMITED : (short)ItemVnums.SELF_INTRODUCTION;

                message = string.Empty;
                valueSplit = guriPacket.Value.Split(' ');
                message = valueSplit.Aggregate(message, (current, t) => current + t + "^");
                message = message[..^1]; // Remove the last ^
                message = message.Trim();
                if (message.Length > 60)
                {
                    message = message.Substring(0, 60);
                }

                session.PlayerEntity.Biography = message;
                session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.INTERACTION_CHATMESSAGE_INTRODUCTION_SET, session.UserLanguage), ChatMessageColorType.Yellow);
                await session.RemoveItemFromInventory(itemVnumToRemove);

                break;

            case 3:
                bool haveLimitedSpeaker = session.PlayerEntity.HasItem((short)ItemVnums.SPEAKER_LIMITED);
                bool haveSpeaker = session.PlayerEntity.HasItem((short)ItemVnums.SPEAKER);
                DateTime now = DateTime.UtcNow;

                if (!haveLimitedSpeaker && !haveSpeaker)
                {
                    return;
                }

                if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
                {
                    session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
                    return;
                }

                if (guriPacket.Value == null)
                {
                    return;
                }

                valueSplit = guriPacket.Value.Split(' ');
                message = valueSplit.Aggregate("", (current, t) => current + t + " ");

                if (session.IsMuted())
                {
                    session.SendMuteMessage();
                    return;
                }


                if (LastMessageTimestamps.TryGetValue(session.PlayerEntity.Id, out DateTime lastMessageTime))
                {
                    if ((now - lastMessageTime).TotalSeconds < 2)
                    {
                        session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.WAIT_TO_USE_ANOTHER_SPEAKER, session.UserLanguage), MsgMessageType.Middle);
                        return;
                    }
                }

                LastMessageTimestamps[session.PlayerEntity.Id] = now;


                await session.EmitEventAsync(new ChatSpeakerEvent(SpeakerType.Normal_Speaker, message));

                if (haveLimitedSpeaker)
                {
                    await session.RemoveItemFromInventory((short)ItemVnums.SPEAKER_LIMITED);
                }
                else
                {
                    await session.RemoveItemFromInventory((short)ItemVnums.SPEAKER);
                }

                break;

            case 4:
                if (session.CantPerformActionOnAct4())
                {
                    return;
                }

                bool haveLimitedBubble = session.PlayerEntity.HasItem((short)ItemVnums.BUBBLE_LIMITED);
                bool haveBubble = session.PlayerEntity.HasItem((short)ItemVnums.BUBBLE);
                if (!haveLimitedBubble && !haveBubble)
                {
                    return;
                }

                if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
                {
                    session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
                    return;
                }
                if (session.IsMuted())
                {
                    session.SendMuteMessage();
                    return;
                }

                session.PlayerEntity.Bubble = DateTime.UtcNow;
                string bubbleMessage = guriPacket.Value;
                if (bubbleMessage.Length > 60)
                {
                    bubbleMessage = bubbleMessage.Substring(0, 60);
                }

                session.PlayerEntity.SaveBubble(bubbleMessage);
                session.SendPacket(new CsprPacket { Message = bubbleMessage });

                if (haveLimitedBubble)
                {
                    await session.RemoveItemFromInventory((short)ItemVnums.BUBBLE_LIMITED);
                }
                else
                {
                    await session.RemoveItemFromInventory((short)ItemVnums.BUBBLE);
                }

                await session.EmitEventAsync(new ChatGenericEvent
                {
                    Message = bubbleMessage,
                    ChatType = ChatType.SpeechBubble
                });
                break;

            case 999:
                haveLimitedSpeaker = session.PlayerEntity.HasItem((short)ItemVnums.SPEAKER_LIMITED);
                haveSpeaker = session.PlayerEntity.HasItem((short)ItemVnums.SPEAKER);
                if (!haveLimitedSpeaker && !haveSpeaker)
                {
                    return;
                }

                if (!Enum.TryParse(guriPacket.Packet[6], out InventoryType type))
                {
                    return;
                }

                if (!short.TryParse(guriPacket.Packet[7], out short slot))
                {
                    return;
                }

                if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
                {
                    session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
                    return;
                }

                if (session.IsMuted())
                {
                    session.SendMuteMessage();
                    return;
                }

                InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(slot, type);

                if (item == null)
                {
                    return;
                }

                string messageItem = $"{guriPacket.Value.Remove(0, 4).Replace(" ", "|")}";

                // check, if itemName is the same as in {}
                string itemName = _languageService.GetLanguage(GameDataType.Item, _itemsManager.GetItem(item.ItemInstance.ItemVNum).Name, session.UserLanguage).Replace(" ", "|");
                int firstItem = messageItem.IndexOf("{", StringComparison.Ordinal) + "{".Length;
                int lastItem = messageItem.LastIndexOf("}", StringComparison.Ordinal);
                string result = messageItem.Substring(firstItem, lastItem - firstItem);
                messageItem = messageItem.Replace(result, "%s");
                messageItem = messageItem.Trim();

                await session.EmitEventAsync(new ChatSpeakerEvent(SpeakerType.Items_Speaker, messageItem, item.ItemInstance));
                if (haveLimitedSpeaker)
                {
                    await session.RemoveItemFromInventory((short)ItemVnums.SPEAKER_LIMITED);
                }
                else
                {
                    await session.RemoveItemFromInventory((short)ItemVnums.SPEAKER);
                }

                break;
        }
    }
}