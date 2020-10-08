using System;

namespace GHent.Models
{
    public abstract class Request
    {
        public Uri DownloadPath { get; set; }
        public string SavePath { get; set; }
    }
}
