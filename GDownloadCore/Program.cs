using GHentDownloaderLibrary.Classes;

namespace GDownloadCore
{
    internal class Program
    {
        private static void Main()
        {
            GAlbum album = new GAlbum
            {
                SaveDirectoryPath = "http://g.e"
            };

            if (album.Parse() != null)
            {
                foreach (GImage image in album.Items)
                {
                    if (image.Parse() == null)
                    {
                        break;
                    }
                }
            }
        }
    }
}