using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.Vehicles;

public interface IVehicleConfigurationProvider
{
    VehicleConfiguration GetByVehicleVnum(int vnum);
    VehicleConfiguration GetByMorph(int morph, GenderType genderType);
}