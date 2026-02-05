using System;

namespace WingsEmu.Game.Act4.Entities;

public class HatusHeads
{
    public HatusHeads(HatusDragonHead dragonHeads) => DragonHeads = dragonHeads;

    public HatusDragonHead DragonHeads { get; }

    public int DragonAttackWidth { get; } = 2;

    public DateTime CastTime { get; set; }
    public HatusDragonHeadState HeadsState { get; set; }
}

public class HatusDragonHead
{
    public short BluePositionX { get; set; }
    public bool BlueIsActive { get; set; }

    public short RedPositionX { get; set; }
    public bool RedIsActive { get; set; }

    public short GreenPositionX { get; set; }
    public bool GreenIsActive { get; set; }
}

public enum HatusDragonHeadState
{
    SHOW = 0,
    IDLE = 1,
    ATTACK_CAST = 2,
    ATTACK_USE = 3,
    TAKING_DAMAGE = 4,
    HIDE_HEAD = 5
}