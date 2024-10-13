using System;

namespace GHent.Shared.Request
{
    public interface IRequest
    {
        public Uri DownloadPath { get; set; }
        public string SavePath { get; set; }
    }
}
