using System.Text.RegularExpressions;

namespace GHent.Models
{
    public static class StringHelper
    {
        private readonly static Regex rgx = new Regex("[/\\\\*?\"<>|]");
        public static string RemoveIllegalCharacters(this string str)
        {
            return rgx.Replace(str, "_");
        }
    }
}