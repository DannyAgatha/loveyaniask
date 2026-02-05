using WingsEmu.DTOs.ServerDatas;

namespace WingsEmu.Game.Items;

public interface IItemBoxManager
{
    ItemBoxDto GetItemBoxByItemVnumAndDesign(int itemVnum);
    void Initialize();
}