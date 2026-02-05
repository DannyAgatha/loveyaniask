using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations.LandOfLife;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Monster;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Npc;

public class RequestNpcPacketHandler : GenericGamePacketHandlerBase<RequestNpcPacket>
{
    private static readonly QuestType[] DialogQuests =
    {
        QuestType.DIALOG,
        QuestType.DIALOG_2,
        QuestType.DELIVER_ITEM_TO_NPC,
        QuestType.GIVE_ITEM_TO_NPC,
        QuestType.GIVE_ITEM_TO_NPC_2,
        QuestType.GIVE_NPC_GOLD,
        QuestType.DIALOG_WHILE_WEARING,
        QuestType.DIALOG_WHILE_HAVING_ITEM,
        QuestType.WIN_RAID_AND_TALK_TO_NPC
    };

    private readonly IDelayManager _delayManager;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly IItemsManager _itemsManager;
    private readonly IQuestManager _questManager;
    private readonly IRecipeManager _recipeManager;
    private readonly ITeleporterManager _teleporterManager;

    public RequestNpcPacketHandler(IDelayManager delayManager, ITeleporterManager teleporterManager, IRecipeManager recipeManager, IGameLanguageService gameLanguageService, IItemsManager itemsManager,
        IQuestManager questManager)
    {
        _delayManager = delayManager;
        _teleporterManager = teleporterManager;
        _recipeManager = recipeManager;
        _gameLanguageService = gameLanguageService;
        _itemsManager = itemsManager;
        _questManager = questManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, RequestNpcPacket packet)
    {
        long owner = packet.TargetNpcId;
        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        if (packet.VisualType == VisualType.Player)
        {
            session.SendTargetNpcDialog(owner, (int)DialogVnums.SHOP_PLAYER);
            return;
        }
        
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(packet.TargetNpcId);
        if (npcEntity == null)
        {
            return;
        }

        if (npcEntity.MinilandOwner != null)
        {
            if (npcEntity.MinilandOwner.Id == session.PlayerEntity.Id)
            {
                session.ChangeMap(session.PlayerEntity.Miniland);
                return;
            }

            IPlayerEntity minilandOwner = npcEntity.MinilandOwner;
            string message = $"{minilandOwner.Name} - {minilandOwner.Session.GetMinilandCleanMessage(_gameLanguageService)}";
            session.SendQnaPacket($"mjoin 1 {minilandOwner.Id} 1", message);
            return;
        }

        #region Quest

        bool showNpcDialog = session.ShowNpcDialog(npcEntity, _questManager);
        IEnumerable<CharacterQuest> npcTalkQuests = session.PlayerEntity.GetCurrentQuestsByTypes(DialogQuests);

        foreach (CharacterQuest npcTalkQuest in npcTalkQuests)
        {
            await session.EmitEventAsync(new QuestNpcTalkEvent(npcTalkQuest, npcEntity, _questManager.IsNpcBlueAlertQuest(npcTalkQuest.QuestId)));
        }

        #endregion

        if (npcEntity.MonsterRaceType == MonsterRaceType.Fixed)
        {
            switch (npcEntity.MonsterRaceSubType)
            {
                case (byte)MonsterSubRace.Fixed.CannonBall:

                    await session.EmitEventAsync(new RainbowBattleCaptureFlagEvent
                    {
                        NpcEntity = npcEntity
                    });

                    return;
                case (byte)MonsterSubRace.Fixed.Unknown3: // collect

                    if (npcEntity.Drops.All(s => s?.MonsterVNum == null))
                    {
                        return;
                    }

                    if (npcEntity.VNumRequired != 0 && npcEntity.AmountRequired != 0)
                    {
                        if (!session.PlayerEntity.HasItem(npcEntity.VNumRequired, npcEntity.AmountRequired))
                        {
                            string itemName = _itemsManager.GetItem(npcEntity.VNumRequired).GetItemName(_gameLanguageService, session.UserLanguage);
                            session.SendMsg(_gameLanguageService.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, npcEntity.AmountRequired, itemName),
                                MsgMessageType.Middle);
                            return;
                        }
                    }


                    DateTime actionDate = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.Mining, TimeSpan.FromSeconds(npcEntity.CollectionDanceTime));

                    session.SendDelay(actionDate.GetTotalMillisecondUntilNow(), npcEntity.NpcVNum is (short)MonsterVnum.ROBBER_GANG_CHEST ? GuriType.OpeningTreasureChest : GuriType.Mining,
                        $"guri 400 {npcEntity.Id}");

                    break;
                case (byte)MonsterSubRace.Fixed.Unknown1: // teleport
                {
                    if (npcEntity.VNumRequired != 0 && npcEntity.AmountRequired != 0)
                    {
                        if (!session.PlayerEntity.HasItem(npcEntity.VNumRequired, npcEntity.AmountRequired))
                        {
                            string itemName = _itemsManager.GetItem(npcEntity.VNumRequired).GetItemName(_gameLanguageService, session.UserLanguage);
                            session.SendMsg(_gameLanguageService.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, npcEntity.AmountRequired, itemName),
                                MsgMessageType.Middle);
                            return;
                        }
                    }

                    TeleporterDTO tp = _teleporterManager.GetTeleportByNpcId(npcEntity.Id)?.FirstOrDefault(t => t?.Type == TeleporterType.TELEPORT_ON_MAP);
                    if (tp == null)
                    {
                        return;
                    }

                    actionDate = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.UseTeleporter);
                    session.SendDelay(actionDate.GetTotalMillisecondUntilNow(), GuriType.ButtonSwitch, $"guri 710 {tp.MapX} {tp.MapY} {npcEntity.Id}");
                    break;
                }
                case (byte)MonsterSubRace.Fixed.MiniLandStructure:
                    IReadOnlyList<Recipe> recipes = _recipeManager.GetRecipesByNpcMonsterVnum(npcEntity.NpcVNum);
                    if (recipes == null)
                    {
                        if (showNpcDialog)
                        {
                            session.SendNpcDialog(npcEntity);
                        }

                        return;
                    }

                    session.SendWopenPacket((byte)WindowType.CRAFTING_ITEMS);
                    session.SendRecipeNpcList(recipes);
                    break;
                default:
                    if (showNpcDialog)
                    {
                        session.SendNpcDialog(npcEntity);
                    }

                    break;
            }

            return;
        }

        if (!showNpcDialog)
        {
            return;
        }

        session.SendNpcDialog(npcEntity);
    }
}