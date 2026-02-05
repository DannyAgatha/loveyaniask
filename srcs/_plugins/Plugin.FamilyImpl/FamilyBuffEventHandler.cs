using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Families;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._enum;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl;

public class FamilyBuffEventHandler : IAsyncEventProcessor<FamilyBuffEvent>
{
    private readonly IFamilyManager _familyManager;
    private readonly IRankingManager _rankingManager;
    private readonly ISessionManager _sessionManager;
    private readonly IItemsManager _itemsManager;
    private readonly IFamilyService _familyService;
    private readonly FamilyConfiguration _familyConfiguration;
    private readonly Act4Configuration _act4Configuration;
    private readonly SerializableGameServer _serializableGameServer;

    public FamilyBuffEventHandler(IFamilyManager familyManager, IRankingManager rankingManager, FamilyConfiguration familyConfiguration, 
        SerializableGameServer serializableGameServer, Act4Configuration act4Configuration, ISessionManager sessionManager, IItemsManager itemsManager, IFamilyService familyService)
    {
        _familyManager = familyManager;
        _rankingManager = rankingManager;
        _familyConfiguration = familyConfiguration;
        _serializableGameServer = serializableGameServer;
        _act4Configuration = act4Configuration;
        _sessionManager = sessionManager;
        _itemsManager = itemsManager;
        _familyService = familyService;
    }
    
    public async Task HandleAsync(FamilyBuffEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        int itemVnum = e.ItemVnum;
        
        if (!session.PlayerEntity.IsInFamily())
        {
            return;
        }

        if (session.PlayerEntity.GetFamilyAuthority() == FamilyAuthority.Member)
        {
            return;
        }

        FamilyBuff config = _familyConfiguration.FamilyBuffs.FirstOrDefault(x => x.ItemVnum == itemVnum);
        if (config == null)
        {
            return;
        }
        
        IFamily family = session.PlayerEntity.Family;
        long? monthlyExpFamilyId = _rankingManager.MonthlyExpFamilyId;
        long? monthlyPvpAngelFamilyId = _rankingManager.MonthlyPvpAngelFamilyId;
        long? monthlyPvpDemonFamilyId = _rankingManager.MonthlyPvpDemonFamilyId;
        long? monthlyRainbowBattleFamilyId = _rankingManager.MonthlyRainbowBattleFamilyId;
            
        bool canUse = family.Id == monthlyExpFamilyId || family.Id == monthlyPvpAngelFamilyId || family.Id == monthlyPvpDemonFamilyId|| family.Id == monthlyRainbowBattleFamilyId;

        if (!canUse)
        {
            return;
        }
            
        var familyFaction = (FactionType)family.Faction;
            
        // Buff already in use
        if (config.BuffVnum.HasValue && _familyManager.CurrentFamilyBuffs.Any(x => x.BuffVnum == config.BuffVnum && (x.FactionType == null || x.FactionType == familyFaction)))
        {
            return;
        }
        
        if (!config.IsFactionRelated)
        {
            if (await _familyManager.AddFamilyBuffAsync(config.ItemVnum, family.Id) == false)
            {
                return;
            }

            await _familyService.CreateFamilyBuffAsync(new FamilyBuffCreateRequest
            {
                FamilyBuffsCrossChannel = new FamilyBuffCrossChannel
                {
                    FamilyId = family.Id,
                    FamilyName = family.Name,
                    ItemVnum = itemVnum,
                    BuffVnum = config.BuffVnum!.Value,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    FactionType = familyFaction
                }
            });
            
            return;
        }

        if (_serializableGameServer.ChannelType != GameChannelType.ACT_4)
        {
            return;
        }

        if (await _familyManager.AddFamilyBuffAsync(config.ItemVnum, family.Id) == false)
        {
            return;
        }

        switch ((ItemVnums)config.ItemVnum)
        {
            case ItemVnums.FAMILY_ACT4_PERCENTAGE:
                int points = (int)(_act4Configuration.MaximumFactionPoints * 0.2);
                await session.EmitEventAsync(new Act4FactionPointsIncreaseEvent((FactionType)family.Faction, points));
                family.SendFmpPacket(_sessionManager, _itemsManager, _familyManager, _rankingManager, _familyConfiguration);
                break;
            
            case ItemVnums.FAMILY_FREEZE_BUFF:
                _familyManager.AddFamilyBuff(new FamilyBuffCrossChannel
                {
                    FamilyId = family.Id,
                    FamilyName = family.Name,
                    ItemVnum = itemVnum,
                    BuffVnum = config.BuffVnum!.Value,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    FactionType = familyFaction
                });
                
                _sessionManager.Broadcast(x => CharacterPacketExtension.GenerateFamilyBuffPacket(config.BuffVnum!.Value, (int)(DateTime.UtcNow.AddHours(1) - DateTime.UtcNow).TotalMilliseconds,true));
                family.SendFmpPacket(_sessionManager, _itemsManager, _familyManager, _rankingManager, _familyConfiguration);
                break;
        }
    }
}