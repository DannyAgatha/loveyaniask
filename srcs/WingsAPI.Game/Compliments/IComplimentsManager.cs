using System.Threading.Tasks;

namespace WingsEmu.Game.Compliments;

public interface IComplimentsManager
{
    Task<bool> CanRefresh(long characterId);
    Task<bool> CanCompliment(long accountId);
}