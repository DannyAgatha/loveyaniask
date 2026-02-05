namespace PhoenixLib.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] byteArray)
        {
            char[] c = new char[byteArray.Length * 2];
            for (int i = 0; i < byteArray.Length; ++i)
            {
                byte b = (byte)(byteArray[i] >> 4);
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = (byte)(byteArray[i] & 0xF);
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }

            return new string(c);
        }
    }
}