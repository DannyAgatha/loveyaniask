// NosEmu
// 


using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WingsEmu.Core.Extensions;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.Vehicles;

public class VehicleConfigurationProvider : IVehicleConfigurationProvider
{
    private readonly ImmutableDictionary<int, VehicleConfiguration> _configurations;
    private readonly Dictionary<int, VehicleConfiguration> _femaleVehicles;
    private readonly Dictionary<int, VehicleConfiguration> _maleVehicles;

    public VehicleConfigurationProvider(IEnumerable<VehicleConfiguration> configurations)
    {
        _configurations = configurations.ToImmutableDictionary(s => s.VehicleVnum);
        _maleVehicles = configurations.GroupBy(s => s.MaleMorphId).ToDictionary(s => s.Key, s => s.First());
        _femaleVehicles = configurations.GroupBy(s => s.FemaleMorphId).ToDictionary(s => s.Key, s => s.First());
    }

    public VehicleConfiguration GetByVehicleVnum(int vnum) => _configurations.GetValueOrDefault(vnum);
    public VehicleConfiguration GetByMorph(int morph, GenderType genderType) => genderType == GenderType.Male ? _maleVehicles.GetOrDefault(morph) : _femaleVehicles.GetOrDefault(morph);
}