using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Communication.Families;
using WingsAPI.Data.Families;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class FrankCtsPacketHandler : GenericGamePacketHandlerBase<FrankCtsPacket>
{
    private readonly IRankingManager _rankingManager;
    private readonly IFamilyService _familyService;
    public FrankCtsPacketHandler(IRankingManager rankingManager, IFamilyService familyService)
    {
        _rankingManager = rankingManager;
        _familyService = familyService;
    }

    protected override async Task HandlePacketAsync(IClientSession session, FrankCtsPacket packet)
    {
        string packetOut = "frank_stc";
        List<(FamilyDTO, long)> famlist = new();

        foreach (FamilyDTO fam in _rankingManager.FamilyRank)
        {
            FamilyReputationRankingResponse familyReputationResponse = await _familyService.GetFamilyReputation(new FamilyReputationRankingRequest() { FamilyId = fam.Id });
            famlist.Add((fam, familyReputationResponse.Reputation));
        }

        int i = 0;
        packetOut += packet.Type switch
        {
            0 or 2 or 3 => " 0",
            4 or 6 or 7 => " 1",
            8 or 9 => " 2",
            _ => string.Empty
        };

        switch (packet.Type)
        {
            case 0:
                famlist = famlist.OrderByDescending(s => s.Item1.CurrentMonthRankStat.ExpEarned).ToList();
                break;
            case 2:
                famlist = famlist.Where(a => a.Item1.Faction == 1).OrderByDescending(s => s.Item1.CurrentMonthRankStat.PvpPoints).ToList();
                break;
            case 3:
                famlist = famlist.Where(a => a.Item1.Faction == 2).OrderByDescending(s => s.Item1.CurrentMonthRankStat.PvpPoints).ToList();
                break;

            case 4:
                famlist = famlist.OrderByDescending(s => s.Item1.PreviousMonthRankStat.ExpEarned).ToList();
                break;

            case 6:
                famlist = famlist.Where(a => a.Item1.Faction == 1).OrderByDescending(s => s.Item1.PreviousMonthRankStat.PvpPoints).ToList();
                break;

            case 7:
                famlist = famlist.Where(a => a.Item1.Faction == 2).OrderByDescending(s => s.Item1.PreviousMonthRankStat.PvpPoints).ToList();
                break;

            case 8:
                famlist = famlist.OrderByDescending(s => s.Item1.Experience).ToList();
                break;

            case 9:
                famlist = famlist.OrderByDescending(s => s.Item2).ToList();
                break;
        }

        foreach ((FamilyDTO, long) fam in famlist.Take(100))
        {
            i++;
            switch (packet.Type)
            {
                case 0:
                    packetOut += $" {i}|{fam.Item1.Name}|{fam.Item1.Level}|{fam.Item1.CurrentMonthRankStat.ExpEarned}";
                    break;

                case 2:
                    packetOut += $" {i}|{fam.Item1.Name}|{fam.Item1.Level}|{fam.Item1.CurrentMonthRankStat.PvpPoints}";
                    break;

                case 3:
                    packetOut += $" {i}|{fam.Item1.Name}|{fam.Item1.Level}|{fam.Item1.CurrentMonthRankStat.PvpPoints}";
                    break;

                case 4:
                    packetOut += $" {i}|{fam.Item1.Name}|{fam.Item1.Level}|{fam.Item1.PreviousMonthRankStat.ExpEarned}";
                    break;

                case 6:
                    packetOut += $" {i}|{fam.Item1.Name}|{fam.Item1.Level}|{fam.Item1.PreviousMonthRankStat.PvpPoints}";
                    break;

                case 7:
                    packetOut += $" {i}|{fam.Item1.Name}|{fam.Item1.Level}|{fam.Item1.PreviousMonthRankStat.PvpPoints}";
                    break;

                case 8:
                    packetOut += $" {i}|{fam.Item1.Name}|{fam.Item1.Level}|{fam.Item1.Experience}";
                    break;

                case 9:
                    packetOut += $" {i}|{fam.Item1.Name}|{fam.Item1.Level}|{fam.Item2}";
                    break;
            }
        }

        session.SendPacket(packetOut);
        session.SendPacket(packetOut);
    }
}