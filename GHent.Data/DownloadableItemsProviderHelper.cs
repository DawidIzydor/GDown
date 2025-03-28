namespace GHent.Data
{
    public static class DownloadableItemsProviderHelper
    {
        public static DownloadableItem? GetByUrlOrDefault(this IDownloadableItemsProvider downloadableItemsProvider, string url)
        {
#pragma warning disable S6602
            return downloadableItemsProvider.Items.FirstOrDefault(i => i.Url == url);
#pragma warning restore S6602
        }
    }
}
