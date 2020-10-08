using HtmlAgilityPack;

namespace GHent.RequestProcessor.Singleton
{
    internal static class HtmlWebSingleton
    {
        public static HtmlWeb Instance { get; } = new HtmlWeb();
    }
}