using GHentDownloaderLibrary.Interfaces;

namespace GHentDownloaderLibrary.Abstract
{
    public interface IParseable<out T> : INetItem
    {
        T Parse();
    }
}