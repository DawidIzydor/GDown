
namespace Ghent.SimplyHentai
{
    public interface IImageSaver
    {
        Task SaveImage(string imageUrl, string savePath, CancellationToken cancellationToken);
    }
}