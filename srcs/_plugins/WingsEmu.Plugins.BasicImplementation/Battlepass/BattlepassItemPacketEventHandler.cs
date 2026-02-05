using PhoenixLib.Events;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.BattlePass;

public class BattlePassItemPacketEventHandler : IAsyncEventProcessor<BattlePassItemPacketEvent>
{
    private readonly BattlePassItemConfiguration _battlePassItemConfiguration;
    private readonly BattlePassBearingConfiguration _battlePassBearingConfiguration;

    public BattlePassItemPacketEventHandler(BattlePassItemConfiguration battlePassItemConfiguration, BattlePassBearingConfiguration battlePassBearingConfiguration)
    {
        _battlePassItemConfiguration = battlePassItemConfiguration;
        _battlePassBearingConfiguration = battlePassBearingConfiguration;
    }
    
    public async Task HandleAsync(BattlePassItemPacketEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        StringBuilder stringBuilder = new($"bpp {(byte)_battlePassBearingConfiguration.Bearings.Count} {session.PlayerEntity.BattlePassOptionDto.Points} {(session.PlayerEntity.BattlePassOptionDto.HavePremium ? 1 : 0)} ");
        foreach (BattlepassBearing bearing in _battlePassBearingConfiguration.Bearings)
        {
            IEnumerable<BattlepassItem> items = _battlePassItemConfiguration.Items.Where(s => s.BearingId == bearing.Id).ToList();;

            if (items.Count() != 2)
            {
                continue;
            }

            BattlepassItem freeItem = items.First(s => !s.IsPremium);
            BattlepassItem premiumItem = items.First(s => s.IsPremium);
            bool freeItemAlreadyTaken = session.PlayerEntity.BattlePassItemDto.Any(s => !s.IsPremium && s.BearingId == bearing.Id);
            bool canGetItem = bearing.MaximumBattlepassPoint <= session.PlayerEntity.BattlePassOptionDto.Points;
            bool premiumItemAlreadyTaken = session.PlayerEntity.BattlePassItemDto.Any(s => s.IsPremium && s.BearingId == bearing.Id);
            stringBuilder.Append($"{(short)bearing.Id} {freeItem.ItemVnum} {freeItem.Amount} {premiumItem.ItemVnum} {premiumItem.Amount} {(byte)(!canGetItem ? 0 : freeItemAlreadyTaken ? 2 : 1)} {(byte)(!canGetItem ? 0 : premiumItemAlreadyTaken ? 2 : 1)} {(premiumItem.IsSuperReward ? 1 : 0)} ");
        }

        session.SendPacket(stringBuilder.ToString());
    }
}