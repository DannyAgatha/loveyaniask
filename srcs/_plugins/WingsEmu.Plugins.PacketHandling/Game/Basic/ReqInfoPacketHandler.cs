using System.Threading.Tasks;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class ReqInfoPacketHandler : GenericGamePacketHandlerBase<ReqInfoPacket>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public ReqInfoPacketHandler(INpcMonsterManager npcMonsterManager, IGameLanguageService gameLanguage,
        IReputationConfiguration reputationConfiguration, ISpPartnerConfiguration spPartnerConfiguration, IRankingManager rankingManager)
    {
        _npcMonsterManager = npcMonsterManager;
        _gameLanguage = gameLanguage;
        _reputationConfiguration = reputationConfiguration;
        _spPartnerConfiguration = spPartnerConfiguration;
        _rankingManager = rankingManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, ReqInfoPacket packet)
    {
        switch (packet.Type)
        {
            case 12:
                InventoryItem item = session.PlayerEntity.GetItemBySlotAndType((short)packet.TargetVNum, InventoryType.Equipment);
                if (item == null)
                {
                    return;
                }

                if (item.ItemInstance.Type != ItemInstanceType.WearableInstance)
                {
                    return;
                }

                if (item.ItemInstance.Rarity <= 0)
                {
                    session.SendPacket($"r_info {item.ItemInstance.ItemVNum} 0");
                    return;
                }

                if (item.ItemInstance.BoundCharacterId == session.PlayerEntity.Id)
                {
                    session.SendPacket($"r_info {item.ItemInstance.ItemVNum} 1");
                    return;
                }

                session.SendPacket($"r_info {item.ItemInstance.ItemVNum} 2");
                break;

            case 6:
                if (!packet.MateVNum.HasValue)
                {
                    return;
                }

                IMonsterData npcPartner = session.CurrentMapInstance.GetMonsterById(packet.MateVNum.Value);
                if (npcPartner != null)
                {
                    session.SendMonsterInfo(npcPartner, _gameLanguage);
                    return;
                }

                IMateEntity targetMate = session.CurrentMapInstance.GetMateById(packet.MateVNum.Value);

                if (targetMate == null)
                {
                    return;
                }

                if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    if (session.PlayerEntity.Faction != targetMate.Faction)
                    {
                        return;
                    }
                }

                session.SendPacket(targetMate.GenerateEInfo(_gameLanguage, session.UserLanguage, _spPartnerConfiguration));
                break;
            case 5:
                IMonsterData npc = _npcMonsterManager.GetNpc((short)packet.TargetVNum);
                if (npc == null)
                {
                    return;
                }

                session.SendNpcInfo(npc, _gameLanguage);

                break;
            default:
                IPlayerEntity target = session.CurrentMapInstance.GetCharacterById(packet.TargetVNum);
                if (target == null)
                {
                    return;
                }

                if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    if (session.PlayerEntity.Faction != target.Faction)
                    {
                        return;
                    }
                }

                if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle && target.RainbowBattleComponent.IsInRainbowBattle)
                {
                    if (session.PlayerEntity.RainbowBattleComponent.Team != target.RainbowBattleComponent.Team)
                    {
                        return;
                    }
                }

                session.SendPacket(target.Session.GenerateReqInfo(_reputationConfiguration, _rankingManager.TopReputation));
                break;
        }
    }
}