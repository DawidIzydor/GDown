using System;
using GHentDownloaderLibrary.Interfaces;

namespace GHentDownloaderLibrary.Abstract
{
    public abstract class CombinedItem : SourceItem, IParseable<CombinedItem>, IFileItem
    {
        public string SaveDirectoryPath { get; set; }
        public string NetPath { get; set; }
        public abstract CombinedItem Parse();

        protected virtual void Verify()
        {
            if (Web == null)
            {
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException(paramName: "RootNode");
            }

        }
    }
}