using System;
using System.Collections.Generic;

namespace Penna.Messaging.Web.Extensions
{
    public static class StringExtensions
    {
        public static string Join(this IEnumerable<string> strings, string separator)
        {
            return String.Join(separator, strings);
        }
    }
}