using HtmlAgilityPack;

namespace GHent.GHentai.Singleton
{
    internal static class HtmlWebSingleton
    {
        public static HtmlWeb Instance { get; } = new HtmlWeb();
    }
}