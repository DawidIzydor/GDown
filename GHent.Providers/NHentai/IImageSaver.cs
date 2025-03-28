namespace Ghent.NHentai
{
    public interface IImageSaver
    {
        Task SaveImage(string imageUrl, string savePath, CancellationToken cancellationToken);
    }
}