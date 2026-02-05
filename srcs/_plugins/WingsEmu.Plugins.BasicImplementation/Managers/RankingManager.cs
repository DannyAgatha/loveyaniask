using PhoenixLib.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Families;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Managers;

public class RankingManager : IRankingManager
{
    private readonly ICharacterService _characterService;
    private readonly IFamilyService _familyService;

    public RankingManager(ICharacterService characterService, IFamilyService familyService)
    {
        _characterService = characterService;
        _familyService = familyService;
    }

    public IReadOnlyList<CharacterDTO> TopCompliment { get; private set; } = new List<CharacterDTO>();
    public IReadOnlyList<CharacterDTO> TopPoints { get; private set; } = new List<CharacterDTO>();
    public IReadOnlyList<CharacterDTO> TopReputation { get; private set; } = new List<CharacterDTO>();
    public IReadOnlyList<FamilyDTO> FamilyRank { get; private set; } = new List<FamilyDTO>();
    public long? MonthlyExpFamilyId { get; private set; }
    public long? MonthlyPvpAngelFamilyId { get; private set; }
    public long? MonthlyPvpDemonFamilyId { get; private set; }
    public long? MonthlyRainbowBattleFamilyId { get; private set; }

    private async Task<IReadOnlyList<FamilyDTO>> GetAndUpdateFamily()
    {
        try
        {
            FamilyAllResponse getAllFam = await _familyService.GetFamilies();
            List<FamilyDTO> families = getAllFam.Families?.ToList() ?? [];
            int day = DateTime.UtcNow.Day;
            foreach (FamilyDTO fam in families)
            {
                if (day == 1)
                {
                    fam.PreviousMonthRankStat.ExpEarned = fam.CurrentMonthRankStat.ExpEarned;
                    fam.PreviousMonthRankStat.PvpPoints = fam.CurrentMonthRankStat.PvpPoints;
                    fam.PreviousMonthRankStat.RainbowBattlePoints = fam.CurrentMonthRankStat.PvpPoints;
                    fam.CurrentMonthRankStat.PvpPoints = 0;
                    fam.CurrentMonthRankStat.ExpEarned = 0;
                    fam.CurrentMonthRankStat.RainbowBattlePoints = 0;
                    fam.CurrentDayRankStat.PvpPoints = 0;
                    fam.CurrentDayRankStat.ExpEarned = 0;
                    fam.CurrentDayRankStat.RainbowBattlePoints = 0;
                }
                else
                {
                    fam.CurrentMonthRankStat.ExpEarned += fam.CurrentDayRankStat.ExpEarned;
                    fam.CurrentMonthRankStat.PvpPoints += fam.CurrentDayRankStat.PvpPoints;
                    fam.CurrentMonthRankStat.RainbowBattlePoints += fam.CurrentDayRankStat.RainbowBattlePoints;
                    fam.GlobalRankStat.ExpEarned += fam.CurrentDayRankStat.ExpEarned;
                    fam.GlobalRankStat.PvpPoints += fam.CurrentDayRankStat.PvpPoints;
                    fam.GlobalRankStat.RainbowBattlePoints += fam.CurrentDayRankStat.RainbowBattlePoints;
                    fam.CurrentDayRankStat.PvpPoints = 0;
                    fam.CurrentDayRankStat.ExpEarned = 0;
                    fam.CurrentDayRankStat.RainbowBattlePoints = 0;
                }

                await _familyService.SaveFamilyAsync(new FamilySaveRequest { FamilyDto = fam });
            }

            MonthlyExpFamilyId = families.MaxBy(x => x.PreviousMonthRankStat.ExpEarned)?.Id;
            MonthlyRainbowBattleFamilyId = families.MaxBy(x => x.PreviousMonthRankStat.RainbowBattlePoints)?.Id;
            MonthlyPvpAngelFamilyId = families.OrderByDescending(x => x.PreviousMonthRankStat.PvpPoints).FirstOrDefault(x => x.Faction is (byte)FactionType.Angel)?.Id;
            MonthlyPvpDemonFamilyId = families.OrderByDescending(x => x.PreviousMonthRankStat.PvpPoints).FirstOrDefault(x => x.Faction is (byte)FactionType.Demon)?.Id;

            return families;
        }
        catch (Exception e)
        {
            Log.Error("[RANKING_MANAGER][GET_AND_UPDATE_FAMILY] Unexpected error:", e);
            return new List<FamilyDTO>();
        }
    }

    public async Task TryRefreshRanking()
    {
        CharacterGetTopResponse response = null;
        try
        {
            response = await _characterService.GetTopCompliment(new EmptyRpcRequest());
        }
        catch (Exception e)
        {
            Log.Error("[RANKING_MANAGER][TRY_REFRESH_RANKING] Unexpected error: ", e);
        }

        if (response?.ResponseType == RpcResponseType.SUCCESS)
        {
            TopCompliment = response.Top ?? new List<CharacterDTO>();
        }

        response = null;
        try
        {
            response = await _characterService.GetTopPoints(new EmptyRpcRequest());
        }
        catch (Exception e)
        {
            Log.Error("[RANKING_MANAGER][TRY_REFRESH_RANKING] Unexpected error: ", e);
        }

        if (response?.ResponseType == RpcResponseType.SUCCESS)
        {
            TopPoints = response.Top ?? new List<CharacterDTO>();
        }

        response = null;
        try
        {
            response = await _characterService.GetTopReputation(new EmptyRpcRequest());
        }
        catch (Exception e)
        {
            Log.Error("[RANKING_MANAGER][TRY_REFRESH_RANKING] Unexpected error: ", e);
        }

        if (response?.ResponseType == RpcResponseType.SUCCESS)
        {
            TopReputation = response.Top ?? new List<CharacterDTO>();
        }

        FamilyRank = await GetAndUpdateFamily();
    }

    public void RefreshRanking(IReadOnlyList<CharacterDTO> topComplimented, IReadOnlyList<CharacterDTO> topPoints, IReadOnlyList<CharacterDTO> topReputation)
    {
        TopCompliment = topComplimented;
        TopPoints = topPoints;
        TopReputation = topReputation;
        FamilyRank = GetAndUpdateFamily().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}