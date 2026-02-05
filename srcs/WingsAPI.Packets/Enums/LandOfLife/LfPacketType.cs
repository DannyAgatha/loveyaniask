namespace WingsAPI.Packets.Enums.LandOfLife;

public enum LfPacketType
{
    /// <summary>
    /// Resets the Land of Life timer display (e.g. on exit).
    /// </summary>
    Reset = 0,

    /// <summary>
    /// Sets the initial Land of Life timer (e.g. on entry).
    /// </summary>
    Set = 1,

    /// <summary>
    /// Updates the remaining time without resetting (e.g. periodic update).
    /// </summary>
    Update = 3
}
