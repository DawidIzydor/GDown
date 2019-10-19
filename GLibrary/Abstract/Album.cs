using System.Collections.Generic;

namespace GHentDownloaderLibrary.Abstract
{
    public abstract class Album<T> : CombinedItem where T : Item
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}