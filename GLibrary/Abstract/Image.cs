namespace GHentDownloaderLibrary.Abstract
{
    public abstract class Image : CombinedItem
    {
        public new Album<Item> Parent { get; set; }
    }
}