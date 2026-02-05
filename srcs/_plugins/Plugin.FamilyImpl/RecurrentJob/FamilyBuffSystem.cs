using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;

namespace Plugin.FamilyImpl.RecurrentJob;

public class FamilyBuffSystem : BackgroundService
{
    private readonly IFamilyManager _familyManager;
    private readonly ISessionManager _sessionManager;
    private readonly IRankingManager _rankingManager;
    private readonly FamilyConfiguration _familyConfiguration;
    private readonly IItemsManager _itemsManager;

    public FamilyBuffSystem(IFamilyManager familyManager, ISessionManager sessionManager, IRankingManager rankingManager, FamilyConfiguration familyConfiguration, IItemsManager itemsManager)
    {
        _familyManager = familyManager;
        _sessionManager = sessionManager;
        _rankingManager = rankingManager;
        _familyConfiguration = familyConfiguration;
        _itemsManager = itemsManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime currentTime = DateTime.UtcNow;
            
            foreach (FamilyBuffCrossChannel familyBuff in _familyManager.CurrentFamilyBuffs.ToArray())
            {
                if (familyBuff.EndTime > currentTime)
                {
                    continue;
                }
                
                _familyManager.RemoveFamilyBuff(familyBuff);
                _sessionManager.Broadcast(x => CharacterPacketExtension.GenerateFamilyBuffPacket(familyBuff.BuffVnum, -1, false));
                IFamily family = _familyManager.GetFamilyByFamilyName(familyBuff.FamilyName);
                family?.SendFmpPacket(_sessionManager, _itemsManager, _familyManager, _rankingManager, _familyConfiguration);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}