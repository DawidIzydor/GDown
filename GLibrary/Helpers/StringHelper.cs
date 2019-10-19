namespace GHentDownloaderLibrary.Helpers
{
    public static class StringHelper
    {
        public static string RemoveIllegalCharacters(string str)
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