using System;

namespace IEC.Common.Other
{
    public static class StringHelper
    {
        public static string GetFromLast(
            this string s,
            int symbolCount
            )
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (s.Length <= symbolCount)
            {
                return s;
            }

            return s.Substring(
                s.Length - symbolCount,
                symbolCount
                );
        }

    }
}
