using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Data.Account;

namespace DatabaseServer.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountBanDao _accountBanDao;
        private readonly IAccountDAO _accountDao;
        private readonly IAccountPenaltyDao _accountPenaltyDao;

        public AccountService(IAccountDAO accountDao, IAccountBanDao accountBanDao, IAccountPenaltyDao accountPenaltyDao)
        {
            _accountDao = accountDao;
            _accountBanDao = accountBanDao;
            _accountPenaltyDao = accountPenaltyDao;
        }

        public async Task<AccountLoadResponse> LoadAccountByName(AccountLoadByNameRequest request)
        {
            AccountDTO dto = null;
            try
            {
                dto = await _accountDao.GetByNameAsync(request.Name);
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_SERVICE][LOAD_ACCOUNT_BY_NAME] Unexpected error: ", e);
            }

            return new AccountLoadResponse
            {
                ResponseType = dto == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                AccountDto = dto
            };
        }

        public async Task<AccountLoadResponse> LoadAccountById(AccountLoadByIdRequest request)
        {
            AccountDTO dto = null;
            try
            {
                dto = await _accountDao.GetByIdAsync(request.AccountId);
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_SERVICE][LOAD_ACCOUNT_BY_ID] Unexpected error: ", e);
            }

            return new AccountLoadResponse
            {
                ResponseType = dto == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                AccountDto = dto
            };
        }

        public async Task<AccountSaveResponse> SaveAccount(AccountSaveRequest request)
        {
            AccountDTO dto = null;
            try
            {
                dto = await _accountDao.SaveAsync(request.AccountDto);
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_SERVICE][SAVE_ACCOUNT] Unexpected error: ", e);
            }

            return new AccountSaveResponse
            {
                ResponseType = dto == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                AccountDto = dto
            };
        }

        public async Task<AccountBanGetResponse> GetAccountBan(AccountBanGetRequest request)
        {
            try
            {
                AccountBanDto dto = await _accountBanDao.FindAccountBan(request.AccountId);

                return new AccountBanGetResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    AccountBanDto = dto
                };
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_SERVICE][GET_ACCOUNT_BAN] Unexpected error: ", e);
            }

            return new AccountBanGetResponse
            {
                ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
            };
        }

        public async Task<AccountBanSaveResponse> SaveAccountBan(AccountBanSaveRequest request)
        {
            AccountBanDto dto = null;
            try
            {
                dto = await _accountBanDao.SaveAsync(request.AccountBanDto);
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_SERVICE][SAVE_ACCOUNT_BAN] Unexpected error: ", e);
            }

            return new AccountBanSaveResponse
            {
                ResponseType = dto == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                AccountBanDto = dto
            };
        }

        public async Task<AccountPenaltyGetAllResponse> GetAccountPenalties(AccountPenaltyGetRequest request)
        {
            try
            {
                List<AccountPenaltyDto> dtos = await _accountPenaltyDao.GetPenaltiesByAccountId(request.AccountId);

                return new AccountPenaltyGetAllResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    AccountPenaltyDtos = dtos
                };
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_SERVICE][GET_ACCOUNT_PENALTIES] Unexpected error: ", e);
            }

            return new AccountPenaltyGetAllResponse
            {
                ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
            };
        }

        public async Task<AccountPenaltyMultiSaveResponse> SaveAccountPenalties(AccountPenaltyMultiSaveRequest request)
        {
            if (request.AccountPenaltyDtos == null)
            {
                return new AccountPenaltyMultiSaveResponse
                {
                    ResponseType = RpcResponseType.SUCCESS
                };
            }

            IEnumerable<AccountPenaltyDto> dtos = null;
            try
            {
                dtos = await _accountPenaltyDao.SaveAsync(request.AccountPenaltyDtos);
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_SERVICE][SAVE_ACCOUNT_PENALTIES] Unexpected error: ", e);
            }

            return new AccountPenaltyMultiSaveResponse
            {
                ResponseType = dtos == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                AccountPenaltyDtos = dtos
            };
        }
        
        public async Task<AccountBanSaveResponse> BanAccount(BanAccountRequest request)
        {
            try
            {
                AccountBanGetResponse banCheckResponse = await GetAccountBan(new AccountBanGetRequest { AccountId = request.AccountId });
                
                if (banCheckResponse.ResponseType == RpcResponseType.SUCCESS && banCheckResponse.AccountBanDto != null)
                {
                    Log.Info($"Account ID [{request.AccountId}] is already banned.");
                    return new AccountBanSaveResponse
                    {
                        ResponseType = RpcResponseType.SUCCESS,
                        AccountBanDto = banCheckResponse.AccountBanDto
                    };
                }
                
                DateTime utcNow = DateTime.UtcNow;
                var newBan = new AccountBanDto
                {
                    JudgeName = "Administrator",
                    Start = utcNow,
                    End = utcNow + TimeSpan.FromDays(362),
                    Reason = request.Reason,
                    AccountId = request.AccountId
                };

                AccountBanDto dto = await _accountBanDao.SaveAsync(newBan);
                
                return new AccountBanSaveResponse
                {
                    ResponseType = dto != null ? RpcResponseType.SUCCESS : RpcResponseType.GENERIC_SERVER_ERROR,
                    AccountBanDto = dto
                };
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_SERVICE][BAN_ACCOUNT] Unexpected error: ", e);
                return new AccountBanSaveResponse { ResponseType = RpcResponseType.GENERIC_SERVER_ERROR };
            }
        }
    }
}