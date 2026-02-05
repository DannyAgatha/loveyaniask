using System.Collections.Generic;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.ServerDatas;

public class ItemBoxDto : IIntDto
{
    public ItemBoxType ItemBoxType { get; set; }

    /// <summary>
    ///     If not set => 1
    /// </summary>
    public int? MinimumRewards { get; set; }

    /// <summary>
    ///     If not set => 1
    /// </summary>
    public int? MaximumRewards { get; set; }

    public bool ShowsRaidBoxPanelOnOpen { get; set; }

    public List<ItemBoxItemDto> Items { get; set; }

    /// <summary>
    ///     ItemVnum
    /// </summary>
    public int Id { get; set; }
}