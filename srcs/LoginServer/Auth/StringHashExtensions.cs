using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WingsAPI.Communication.Auth;

namespace LoginServer.Auth
{
    internal static class StringHashExtensions
    {
        public static string ToSha256(this string str)
        {
            using var hashing = SHA256.Create();
            return string.Concat(hashing.ComputeHash(Encoding.UTF8.GetBytes(str)).Select((Func<byte, string>)(item => item.ToString("x2"))));
        }

        public static string GetComputedClientHash(this AuthorizedClientVersionDto clientVersion)
        {
            string dllHash = clientVersion.DllHash;
            string executableHash = clientVersion.ExecutableHash;

            return (executableHash + dllHash).ToSha256();
        }
        
        public static string DecodeHexString(this string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];
            
            for (int i = 0; i < hexString.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }
}