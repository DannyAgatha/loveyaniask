namespace WingsEmu.Game.Maps;

public interface IMapInstanceFactory
{
    IMapInstance CreateMap(Map map, MapInstanceType mapInstanceType);
}