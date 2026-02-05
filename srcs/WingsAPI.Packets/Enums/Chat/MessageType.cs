namespace WingsAPI.Packets.Enums.Chat;

public enum MessageType : byte
{
    Default = 0,
    Notification = 1,
    Shout = 2,
    Center = 3,
    Hero = 4,
    DefaultAndNotification
}