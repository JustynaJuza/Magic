using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Magic.Models.Helpers
{
    public static class StringExtensions
    {
        public static string SplitCamelCase(this string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }
    }
}