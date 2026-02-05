using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PhoenixLib.Configuration
{
    public static partial class StringExtensions
    {
        public static string ToUnderscoreCase(this string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
        }
        
        [GeneratedRegex("([a-z])([A-Z])")]
        private static partial Regex CreateCamelCaseRegex();
        
        public static string AddSpacesToCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            
            Regex camelCaseRegex = CreateCamelCaseRegex();
            return camelCaseRegex.Replace(input, "$1 $2");
        }
        
        public static string FormatElapsedTime(this TimeSpan timeElapsed)
        {
            string formattedTime = "";
            
            if (timeElapsed.TotalHours >= 1)
            {
                formattedTime += $"{timeElapsed.Hours}h ";
            }
            
            if (timeElapsed.TotalMinutes >= 1)
            {
                formattedTime += $"{timeElapsed.Minutes}m ";
            }
            
            formattedTime += $"{timeElapsed.Seconds}s";
            
            return formattedTime.Trim();
        }
    }
}