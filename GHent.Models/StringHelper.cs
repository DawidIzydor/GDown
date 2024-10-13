using System.Text.RegularExpressions;

namespace GHent.Models
{
    public static class StringHelper
    {
        // TODO: replace with Regex
        public static string RemoveIllegalCharacters(this string str)
        {
            string ret = str;
            ret = ret.Replace(':', '_');
            ret = ret.Replace('/', '_');
            ret = ret.Replace('\\', '_');
            ret = ret.Replace('*', '_');
            ret = ret.Replace('?', '_');
            ret = ret.Replace('"', '_');
            ret = ret.Replace('<', '_');
            ret = ret.Replace('>', '_');
            ret = ret.Replace('|', '_');
            return ret;
        }
    }
}