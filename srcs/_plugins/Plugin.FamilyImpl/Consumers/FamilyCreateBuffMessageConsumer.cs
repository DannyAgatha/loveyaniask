using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;

namespace Plugin.FamilyImpl.Consumers;

public class FamilyCreateBuffMessageConsumer : IMessageConsumer<FamilyCreateBuffMessage>
{
    private readonly IFamilyManager _familyManager;
    private readonly IRankingManager _rankingManager;
    private readonly ISessionManager _sessionManager;
    private readonly IItemsManager _itemsManager;
    private readonly FamilyConfiguration _familyConfiguration;

    public FamilyCreateBuffMessageConsumer(IFamilyManager familyManager, IRankingManager rankingManager, FamilyConfiguration familyConfiguration, ISessionManager sessionManager, IItemsManager itemsManager)
    {
        _familyManager = familyManager;
        _rankingManager = rankingManager;
        _familyConfiguration = familyConfiguration;
        _sessionManager = sessionManager;
        _itemsManager = itemsManager;
    }
    
    public async Task HandleAsync(FamilyCreateBuffMessage notification, CancellationToken token)
    {
        _familyManager.AddFamilyBuff(new FamilyBuffCrossChannel
        {
            FamilyId = notification.FamilyId,
            FamilyName = notification.FamilyName,
            ItemVnum = notification.ItemVnum,
            BuffVnum = notification.BuffVnum,
            EndTime = notification.EndTime,
            FactionType = notification.FactionType
        });
                
        _sessionManager.Broadcast(x => CharacterPacketExtension.GenerateFamilyBuffPacket(notification.BuffVnum, (int)(notification.EndTime - DateTime.UtcNow).TotalMilliseconds, true));
        IFamily family = _familyManager.GetFamilyByFamilyName(notification.FamilyName);
        family.SendFmpPacket(_sessionManager, _itemsManager, _familyManager, _rankingManager, _familyConfiguration);
    }
}