using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;
using WingsAPI.Packets.Enums.LandOfDeath;

namespace Plugin.LandOfDeath.Handlers;

public class RevivalEventLandOfDeathHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly LandOfDeathConfiguration _landOfDeathConfiguration;
    private readonly IGameLanguageService _languageService;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;
    private readonly ILandOfDeathManager _landOfDeathManager;

    public RevivalEventLandOfDeathHandler(
        LandOfDeathConfiguration landOfDeathConfiguration,
        IGameLanguageService languageService,
        ISpPartnerConfiguration spPartnerConfiguration,
        ILandOfDeathManager landOfDeathManager)
    {
        _landOfDeathConfiguration = landOfDeathConfiguration;
        _languageService = languageService;
        _spPartnerConfiguration = spPartnerConfiguration;
        _landOfDeathManager = landOfDeathManager;
    }

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (session.PlayerEntity.IsAlive() || session.CurrentMapInstance is not { MapInstanceType: MapInstanceType.LandOfDeath })
        {
            return;
        }

        bool hasPaidPenalization = false;
        if (e.RevivalType == RevivalType.TryPayRevival)
        {
            for (int i = 0; i < _landOfDeathConfiguration.SpawnItemVnums.Count; i++)
            {
                int vnum = _landOfDeathConfiguration.SpawnItemVnums[i];
                short amount = 1;
                
                if (_landOfDeathConfiguration.SpawnItemAmounts != null 
                    && i < _landOfDeathConfiguration.SpawnItemAmounts.Count 
                    && _landOfDeathConfiguration.SpawnItemAmounts[i] > 0)
                {
                    amount = _landOfDeathConfiguration.SpawnItemAmounts[i];
                }

                if (!session.PlayerEntity.HasItem(vnum, amount))
                {
                    continue;
                }

                await session.RemoveItemFromInventory(vnum, amount);
                hasPaidPenalization = true;
                break;
            }
        }

        if (hasPaidPenalization)
        {
            session.PlayerEntity.Hp = e.Sender.PlayerEntity.MaxHp;
            session.PlayerEntity.Mp = e.Sender.PlayerEntity.MaxMp;
            session.RefreshStat();
            session.BroadcastTeleportPacket();
            session.BroadcastInTeamMembers(_languageService, _spPartnerConfiguration);
            session.RefreshParty(_spPartnerConfiguration);
            
            LandOfDeathInstance currentInstance = _landOfDeathManager.Instances
                .FirstOrDefault(i => i.MapInstance.Id == session.CurrentMapInstance.Id);

            if (currentInstance != null)
            {
                LandOfDeathMode mode = currentInstance.Mode;

                LandOfDeathInstance instanceFamily = null;
                if (e.Sender.PlayerEntity.Family?.Id != null)
                {
                    instanceFamily = _landOfDeathManager.GetLandOfDeathInstanceByFamilyId(e.Sender.PlayerEntity.Family.Id, mode);
                }

                LandOfDeathInstance instanceGroup = _landOfDeathManager.GetLandOfDeathInstanceByGroupId(e.Sender.PlayerEntity.Id, mode);
                LandOfDeathInstance instanceSolo = _landOfDeathManager.GetLandOfDeathInstanceByPlayerId(e.Sender.PlayerEntity.Id);
                LandOfDeathInstance instancePublic = _landOfDeathManager.GetLandOfDeathInstanceByPublic(mode);

                LandOfDeathInstance[] instances = { instanceFamily, instanceGroup, instanceSolo, instancePublic };

                foreach (LandOfDeathInstance instance in instances)
                {
                    if (instance is null)
                    {
                        continue;
                    }

                    instance.LastPlayerId = session.PlayerEntity.Id;
                    break;
                }
            }
        }
        else
        {
            session.PlayerEntity.Hp = 1;
            session.PlayerEntity.Mp = 1;
            session.RefreshStat();
            session.ChangeToLastBaseMap();
        }

        session.BroadcastRevive();
        session.UpdateVisibility();
        await session.CheckPartnerBuff();
        session.SendBuffsPacket();
    }
}
