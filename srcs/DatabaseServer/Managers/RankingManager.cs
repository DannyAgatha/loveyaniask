using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using ProtoBuf;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;

namespace DatabaseServer.Managers
{
    public class RankingManager : IRankingManager
    {
        private readonly ICharacterDAO _characterDao;
        private readonly IFamilyDAO _familyDAO;

        private IReadOnlyList<CharacterDTO> _topCompliment;
        private IReadOnlyList<CharacterDTO> _topPoints;
        private IReadOnlyList<CharacterDTO> _topReputation;

        private IReadOnlyList<FamilyDTO> _globalFamily;

        public RankingManager(ICharacterDAO characterDao, IFamilyDAO familyDAO)
        {
            _characterDao = characterDao;
            _familyDAO = familyDAO;
        }

        public async Task<IReadOnlyList<CharacterDTO>> GetTopCompliment()
        {
            if (_topCompliment != null)
            {
                return _topCompliment;
            }

            try
            {
                _topCompliment = await _characterDao.GetTopCompliment();
            }
            catch (Exception e)
            {
                Log.Error("[RANKING_MANAGER][GET_TOP_COMPLIMENT] Unexpected error:", e);
            }

            return _topCompliment;
        }

        public async Task<IReadOnlyList<CharacterDTO>> GetTopPoints()
        {
            if (_topPoints != null)
            {
                return _topPoints;
            }

            try
            {
                _topPoints = await _characterDao.GetTopPoints();
            }
            catch (Exception e)
            {
                Log.Error("[RANKING_MANAGER][GET_TOP_POINTS] Unexpected error:", e);
            }

            return _topPoints;
        }

        public async Task<IReadOnlyList<CharacterDTO>> GetTopReputation()
        {
            if (_topReputation != null)
            {
                return _topReputation;
            }

            try
            {
                _topReputation = await _characterDao.GetTopReputation();
            }
            catch (Exception e)
            {
                Log.Error("[RANKING_MANAGER][GET_TOP_REPUTATION] Unexpected error:", e);
            }

            return _topReputation;
        }

        public async Task<IReadOnlyList<FamilyDTO>> GetAndUpdateFamily()
        {
            try
            {
                IEnumerable<FamilyDTO> getAllFam = await _familyDAO.GetAllAsync();
                var families = getAllFam.ToList();
                int day = DateTime.UtcNow.Day;
                foreach (FamilyDTO fam in families)
                {
                    if (day == 1)
                    {
                        fam.PreviousMonthRankStat.ExpEarned = fam.CurrentMonthRankStat.ExpEarned;
                        fam.PreviousMonthRankStat.PvpPoints = fam.CurrentMonthRankStat.PvpPoints;
                        fam.PreviousMonthRankStat.RainbowBattlePoints = fam.CurrentMonthRankStat.RainbowBattlePoints;
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
                }

                _globalFamily = families;
            }
            catch (Exception e)
            {
                Log.Error("[RANKING_MANAGER][GET_AND_UPDATE_FAMILY] Unexpected error:", e);
                
            }

            return _globalFamily;
        }
        
        
        public async Task<RefreshResponse> TryRefreshRanking()
        {
            try
            {
                _topCompliment = await _characterDao.GetTopCompliment();
            }
            catch (Exception e)
            {
                Log.Error("[RANKING_MANAGER][TRY_REFRESH_RANKING] Unexpected error:", e);
                return new RefreshResponse
                {
                    Success = false
                };
            }

            try
            {
                _topPoints = await _characterDao.GetTopPoints();
            }
            catch (Exception e)
            {
                Log.Error("[RANKING_MANAGER][TRY_REFRESH_RANKING] Unexpected error:", e);
                return new RefreshResponse
                {
                    Success = false
                };
            }

            try
            {
                _topReputation = await _characterDao.GetTopReputation();
            }
            catch (Exception e)
            {
                Log.Error("[RANKING_MANAGER][TRY_REFRESH_RANKING] Unexpected error:", e);
                return new RefreshResponse
                {
                    Success = false
                };
            }

            try
            {
                _globalFamily = await GetAndUpdateFamily();
            }
            catch (Exception e)
            {
                Log.Error("[RANKING_MANAGER][TRY_REFRESH_RANKING] Unexpected error:", e);
                return new RefreshResponse
                {
                    Success = false
                };
            }

            return new RefreshResponse
            {
                Success = true,
                TopCompliment = _topCompliment,
                TopPoints = _topPoints,
                TopReputation = _topReputation,
                FamilyRank = _globalFamily,
            };
        }
    }
}