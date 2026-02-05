// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Game.Shops;

namespace WingsEmu.Game.Managers.ServerData;

public interface IShopManager
{
    Task InitializeAsync();
    ShopNpc GetShopByNpcId(int npcId);
}