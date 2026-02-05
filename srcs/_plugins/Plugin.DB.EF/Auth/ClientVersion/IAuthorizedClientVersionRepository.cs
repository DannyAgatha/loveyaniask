using System.Threading.Tasks;
using PhoenixLib.DAL;
using WingsAPI.Communication.Auth;

namespace Plugin.Database.Auth.ClientVersion
{
    public interface IAuthorizedClientVersionRepository : IGenericAsyncLongRepository<AuthorizedClientVersionDto>
    {
        Task<AuthorizedClientVersionDto> DeleteAsync(string clientVersion);
    }
}