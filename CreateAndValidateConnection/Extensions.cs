﻿namespace CreateAndValidateConnections
{
    public static class StringExtensions
    {
        public static string EnsureEndsWith(this string str, string endsWith)
        {
            // TODO asserts

            return !str.EndsWith(endsWith)
                ? str + "/"
                : str;
        }
    }
}
