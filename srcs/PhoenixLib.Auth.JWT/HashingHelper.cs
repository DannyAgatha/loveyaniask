using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PhoenixLib.Auth.JWT
{
    internal static class HashingHelper
    {
        internal static string ToSha512(this string str)
        {
            using var hash = SHA512.Create();
            return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(str)).Select(item => item.ToString("x2")));
        }
    }
}