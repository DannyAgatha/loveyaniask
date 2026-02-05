// NosEmu
// 


namespace WingsEmu.DTOs.Account;

public enum AuthorityType : short
{
    Closed = -3,
    Banned = -2,
    Unconfirmed = -1,
    User = 0,
    VIP = 1,
    VIP_E = 3,
    VIP_X = 5,
    SUP = 10,
    SUP_E = 15,
    SUP_X = 20,
    GS = 25,
    BetaGameTester = 30,
    GM = 40,
    SGM = 500,


    CM = 900,

    GA = 1000, // everything???

    Owner = 1337, // everything except giving rights & some Remote


    DEV = 30000 // everything
}