using HtmlAgilityPack;

namespace GHentDownloaderLibrary.Abstract
{
    public abstract class SourceItem : Item
    {
        public HtmlWeb Web { get; set; }
    }
}