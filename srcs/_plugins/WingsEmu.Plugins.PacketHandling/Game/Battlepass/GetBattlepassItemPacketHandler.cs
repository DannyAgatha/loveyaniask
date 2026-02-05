using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Data.BattlePass;
using WingsAPI.Packets.ClientPackets;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Mails;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.PacketHandling.Game.BattlePass
{
    public class GetBattlePassItemPacketHandler : GenericGamePacketHandlerBase<BpPSelPacket>
    {
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly BattlePassItemConfiguration _battlePassItemConfiguration;
        private readonly BattlePassBearingConfiguration _battlePassBearingConfiguration;
        private readonly BattlePassConfiguration _battlePassConfiguration;

        public GetBattlePassItemPacketHandler(IGameItemInstanceFactory gameInstanceFactory, BattlePassItemConfiguration battlePassItemConfiguration, 
            BattlePassBearingConfiguration battlePassBearingConfiguration, BattlePassConfiguration battlePassConfiguration)
        {
            _gameItemInstanceFactory = gameInstanceFactory;
            _battlePassItemConfiguration = battlePassItemConfiguration;
            _battlePassBearingConfiguration = battlePassBearingConfiguration;
            _battlePassConfiguration = battlePassConfiguration;
        }

        protected override async Task HandlePacketAsync(IClientSession session, BpPSelPacket packet)
        {
            packet.BearingId++;
            if (packet.Type != BattlePassItemType.All)
            {
                bool alreadyTaken = session.PlayerEntity.BattlePassItemDto.Any(s => s.IsPremium == (packet.Type == BattlePassItemType.Premium) && s.BearingId == packet.BearingId);

                if (alreadyTaken) 
                {
                    return;
                }

                BattlepassItem exist = _battlePassItemConfiguration.Items.Find(s => s.BearingId == packet.BearingId && s.IsPremium == (packet.Type == BattlePassItemType.Premium));

                if (exist == null)
                {
                    return;                                
                }

                if (exist.IsPremium && !session.PlayerEntity.BattlePassOptionDto.HavePremium)
                {
                    return;
                }

                BattlepassBearing getBearing = _battlePassBearingConfiguration.Bearings.FirstOrDefault(s => s.Id == packet.BearingId);

                if (getBearing == null)
                {
                    return;
                }

                if (getBearing.MaximumBattlepassPoint > session.PlayerEntity.BattlePassOptionDto.Points)
                {
                    return;
                }
            }

            switch (packet.Type)
            {
                case BattlePassItemType.All:
                    foreach (BattlepassItem item in _battlePassItemConfiguration.Items)
                    {
                        if (item.IsPremium && !session.PlayerEntity.BattlePassOptionDto.HavePremium)
                        {
                            continue;
                        }

                        bool alreadyTaken = session.PlayerEntity.BattlePassItemDto.Any(s => s.IsPremium == item.IsPremium && s.BearingId == item.BearingId);

                        if (alreadyTaken)
                        {
                            continue;
                        }

                        BattlepassBearing bearing = _battlePassBearingConfiguration.Bearings.FirstOrDefault(s => s.Id == item.BearingId);

                        if (bearing == null)
                        {
                            continue;
                        }

                        if (bearing.MaximumBattlepassPoint > session.PlayerEntity.BattlePassOptionDto.Points)
                        {
                            continue;
                        }

                        GameItemInstance itemInstance = _gameItemInstanceFactory.CreateItem(item.ItemVnum, item.Amount);
                        await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, session.PlayerEntity.Id, MailGiftType.Normal, itemInstance));

                        session.PlayerEntity.BattlePassItemDto.Add(new BattlePassItemDto
                        {
                            BearingId = item.BearingId,
                            IsPremium = item.IsPremium
                        });
                    }
                    break;

                default:
                    BattlepassItem item2 = _battlePassItemConfiguration.Items.FirstOrDefault(s => s.BearingId == packet.BearingId && s.IsPremium == (packet.Type == BattlePassItemType.Premium));

                    if (item2 == null)
                    {
                        return;
                    }

                    if (item2.IsPremium && !session.PlayerEntity.BattlePassOptionDto.HavePremium)
                    {
                        return;
                    }

                    BattlepassBearing bearing2 = _battlePassBearingConfiguration.Bearings.FirstOrDefault(s => s.Id == item2.BearingId);

                    if (bearing2 == null)
                    {
                        return;
                    }

                    if (bearing2.MaximumBattlepassPoint > session.PlayerEntity.BattlePassOptionDto.Points)
                    {
                        return;
                    }

                    GameItemInstance itemInstance2 = _gameItemInstanceFactory.CreateItem(item2.ItemVnum, item2.Amount);
                    await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, session.PlayerEntity.Id, MailGiftType.Normal, itemInstance2));

                    session.PlayerEntity.BattlePassItemDto.Add(new BattlePassItemDto
                    {
                        BearingId = item2.BearingId,
                        IsPremium = item2.IsPremium
                    });

                    break;
            }

            await session.EmitEventAsync(new BattlePassQuestPacketEvent());
            await session.EmitEventAsync(new BattlePassItemPacketEvent());
            session.SendPacket(new BptPacket
            {
                MinutesUntilSeasonEnd = (long)Math.Round((_battlePassConfiguration.EndSeason - DateTime.Now).TotalMinutes)
            });
        }
    }
}