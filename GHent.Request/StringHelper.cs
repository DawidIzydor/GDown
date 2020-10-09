namespace GHent.RequestProcessor
{
    internal static class StringHelper
    {
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