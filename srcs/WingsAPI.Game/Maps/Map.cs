// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.DTOs.Maps;

namespace WingsEmu.Game.Maps;

public class Map
{
    public List<MapFlags> Flags { get; init; }
    public IReadOnlyList<byte> Grid { get; init; }
    public int MapId { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int MapVnum { get; init; }
    public int MapNameId { get; init; }
    public int Music { get; init; }
    
    public bool IsBossRoom { get; init; }
}