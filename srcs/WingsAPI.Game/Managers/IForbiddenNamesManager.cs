// NosEmu
// 


using System.Threading.Tasks;

namespace WingsEmu.Game.Managers;

public interface IForbiddenNamesManager
{
    bool IsBanned(string name, out string s);
    Task Reload();
}