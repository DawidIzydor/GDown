using System;

namespace GHent.Models
{
    public interface IRequest
    {
        public Uri DownloadPath { get; set; }
        public string SavePath { get; set; }
    }
}
