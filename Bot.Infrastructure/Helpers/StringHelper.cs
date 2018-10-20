using System;

namespace Bot.Infrastructure.Helpers
{
    public static class StringHelper
    {
        public static bool CaseInsensitiveContains(this string text, string value, 
            StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }
    }
}