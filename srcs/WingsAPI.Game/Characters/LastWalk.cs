using System;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Characters;

public class LastWalk
{
    public DateTime WalkTimeStart { get; set; }
    public Position StartPosition { get; set; }
    public DateTime WalkTimeEnd { get; set; }
    public Position EndPosition { get; set; }
    public int? MapId { get; set; }
}