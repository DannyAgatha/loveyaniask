// NosEmu
// 


namespace WingsEmu.Packets.Enums
{
    public enum ReceiverType : byte
    {
        Unknown = 0,
        All = 1,
        AllExceptMe = 2,
        AllInRange = 3,
        AllNoEmoBlocked = 5,
        AllExceptGroup = 8,
        AllExceptMeAct4 = 9
    }
}