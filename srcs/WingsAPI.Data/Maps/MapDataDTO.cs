// NosEmu
// 


using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Maps;

public class MapDataDTO : IIntDto
{
    public short Height { get; set; }
    public short Width { get; set; }
    public byte[] Grid { get; set; }
    public string Name { get; set; }
    public int Id { get; set; }
}