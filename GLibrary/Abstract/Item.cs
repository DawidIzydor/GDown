namespace GHentDownloaderLibrary.Abstract
{
    public abstract class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Item Parent { get; set; }
    }
}