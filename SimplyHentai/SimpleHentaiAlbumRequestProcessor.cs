using GHent.Models;


namespace Ghent.SimplyHentai
{
    public class SimpleHentaiAlbumRequestProcessor : IRequestProcessor
    {
        private IProgress<DownloadProgressReport> progress;

        public SimpleHentaiAlbumRequestProcessor(IProgress<DownloadProgressReport> progress)
        {
            this.progress = progress;
        }

        public Task<string> Download(IRequest request, CancellationToken cancellationToken)
        {
            throw new Exception();
        }
    }
}