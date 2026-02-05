using WingsEmu.DTOs.Maps;

namespace NosEmu.Plugins.BasicImplementations.Vehicles;

public class VehicleMapSpeed
{
    /// <summary>
    ///     MapTypeId for the bonus
    /// </summary>
    public MapFlags MapFlag { get; set; }

    /// <summary>
    ///     Bonus to apply to the default speed
    /// </summary>
    public int SpeedBonus { get; set; }
}