// NosEmu
// 


namespace LoginServer.Utils
{
    public interface ISpamProtector
    {
        bool CanConnect(string ipAddress);
    }
}