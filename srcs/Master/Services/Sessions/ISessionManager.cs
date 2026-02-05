using System.Threading.Tasks;
using WingsAPI.Communication.Sessions.Model;

namespace WingsEmu.Master.Sessions
{
    public interface ISessionManager
    {
        Task<Session> GetSessionBySessionId(string sessionId);
        Task<bool> Create(Session session);
        Task<bool> Update(Session session);
        Task<Session> GetSessionByAccountName(string accountName);
        Task<Session> GetSessionByAccountId(long accountId);
        Task<bool> Pulse(Session session);
    }
}