using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace PhoenixLib.Auth.JWT
{
    public static class JwtExtensions
    {
        public static bool EqualsTo(this Claim x, Claim y)
        {
            if (x == null || y == null || x == y)
            {
                return true;
            }

            if (x.Type == y.Type && x.Value == y.Value)
            {
                return true;
            }

            return false;
        }

        public static bool ContainsAll(this IEnumerable<Claim> values, IEnumerable<Claim> expectedValues)
        {
            int initialAmountOfValues = expectedValues.Count();
            var equalValues = new HashSet<string>();
            foreach (Claim value in values)
            {
                foreach (Claim value2 in expectedValues)
                {
                    if (value.EqualsTo(value2))
                    {
                        equalValues.Add(value2.ToString());
                    }

                    if (equalValues.Count >= initialAmountOfValues)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool Contains(this IEnumerable<Claim> values, Claim expectedValue)
        {
            foreach (Claim value in values)
            {
                if (value.EqualsTo(expectedValue))
                {
                    return true;
                }
            }

            return false;
        }
    }
}