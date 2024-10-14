using System.Text.RegularExpressions;

namespace GHent.Shared
{
    public static class StringHelper
    {
        private readonly static Regex rgx = new("[\\W]");
        public static string RemoveIllegalCharacters(this string str)
        {
            return rgx.Replace(str, "_");
        }
    }
}