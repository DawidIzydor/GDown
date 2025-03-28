
namespace GHent.Data
{
    public interface IDownloadableItemsProvider
    {
        IReadOnlyCollection<DownloadableItem> Items { get; }

        void AddItem(DownloadableItem item);
        Task SaveChangesAsync();
    }
}