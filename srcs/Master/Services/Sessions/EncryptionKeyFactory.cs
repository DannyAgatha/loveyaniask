namespace WingsEmu.Master.Sessions
{
    public class EncryptionKeyFactory
    {
        private int _key;

        public int CreateEncryptionKey()
        {
            _key += 2;
            return _key;
        }
    }
}