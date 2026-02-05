using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ServiceContract]
    public interface IAccountService
    {
        /*
         * AccountDTO
         */
        [OperationContract]
        Task<AccountLoadResponse> LoadAccountByName(AccountLoadByNameRequest request);

        [OperationContract]
        Task<AccountLoadResponse> LoadAccountById(AccountLoadByIdRequest request);

        [OperationContract]
        Task<AccountSaveResponse> SaveAccount(AccountSaveRequest request);

        /*
         * AccountBanDTO
         */
        [OperationContract]
        Task<AccountBanGetResponse> GetAccountBan(AccountBanGetRequest request);

        [OperationContract]
        Task<AccountBanSaveResponse> SaveAccountBan(AccountBanSaveRequest request);

        /*
         * AccountPenaltyDTO
         */
        [OperationContract]
        Task<AccountPenaltyGetAllResponse> GetAccountPenalties(AccountPenaltyGetRequest request);

        [OperationContract]
        Task<AccountPenaltyMultiSaveResponse> SaveAccountPenalties(AccountPenaltyMultiSaveRequest request);
        
        [OperationContract]
        Task<AccountBanSaveResponse> BanAccount(BanAccountRequest request);
    }
}