using HtmlAgilityPack;
using System;

namespace GHent.GHentai.Singleton
{
    [Obsolete(message: "We should use dependency injection instead")]
    internal static class HtmlWebSingleton
    {
        public static HtmlWeb Instance { get; } = new HtmlWeb();
    }
}