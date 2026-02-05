using System;

namespace WingsEmu.Game._enum;

[Flags]
public enum BuffFlag
{
    NORMAL = 1 << 0, // Normal buff, ends after x sec/min
    BIG = 1 << 1, // Use vb packet instead of bf, doesn't disappear after death/transform SP
    NO_DURATION = 1 << 2, // No duration - refreshes automatically if it somehow ends
    SAVING_ON_DISCONNECT = 1 << 3, // The duration is kept after logging out
    REFRESH_AT_EXPIRATION = 1 << 4,
    NOT_REMOVED_ON_DEATH = 1 << 5,
    NOT_REMOVED_ON_SP_CHANGE = 1 << 6,
    DISAPPEAR_ON_PVP = 1 << 7,

    BIG_AND_KEEP_ON_LOGOUT = BIG | SAVING_ON_DISCONNECT,
    PARTNER = NORMAL | NO_DURATION
}