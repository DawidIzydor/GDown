using GHent.Shared.ProgressReporter;

namespace Ghent.SimplyHentai
{
    public class HttpClientImageSaver(IProgressReporter<ProgressData<string>>? progressReporter = default) : IImageSaver
    {

        public async Task SaveImage(string imageUrl, string savePath, CancellationToken cancellationToken)
        {
            // Download the image
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(imageUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            await File.WriteAllBytesAsync(savePath, imageBytes, cancellationToken);

            progressReporter?.Report(new ProgressData<string>
            {
                Value = savePath,
                Type = ProgressType.Success
            });
        }
    }
}